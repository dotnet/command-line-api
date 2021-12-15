// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Completions;
using System.IO;
using System.Linq;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Formats output to be shown to users to describe how to use a command line tool.
    /// </summary>
    public class HelpBuilder 
    {
        private const string Indent = "  ";

        private Dictionary<ISymbol, Customization>? _customizationsBySymbol;
        private Func<HelpContext, IEnumerable<HelpSectionDelegate>>? _getLayout;

        /// <param name="localizationResources">Resources used to localize the help output.</param>
        /// <param name="maxWidth">The maximum width in characters after which help output is wrapped.</param>
        public HelpBuilder(
            LocalizationResources localizationResources, 
            int maxWidth = int.MaxValue)
        {
            LocalizationResources = localizationResources ?? throw new ArgumentNullException(nameof(localizationResources));

            if (maxWidth <= 0)
            {
                maxWidth = int.MaxValue;
            }
            MaxWidth = maxWidth;
        }

        /// <summary>
        /// Provides localizable strings for help and error messages.
        /// </summary>
        public LocalizationResources LocalizationResources { get; }

        /// <summary>
        /// The maximum width for which to format help output.
        /// </summary>
        public int MaxWidth { get; }

        /// <summary>
        /// Writes help output for the specified command.
        /// </summary>
        public virtual void Write(HelpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Command.IsHidden)
            {
                return;
            }

            if (OnCustomize is {})
            {
                OnCustomize(context);
            }

            foreach (var writeSection in GetLayout(context))
            {
                writeSection(context);

                if (!context.WasSectionSkipped)
                {
                    context.Output.WriteLine();
                }
            }

            context.Output.WriteLine();
        }

        internal Action<HelpContext>? OnCustomize { get; set; }

        /// <summary>
        /// Gets the default sections to be written for command line help.
        /// </summary>
        public static IEnumerable<HelpSectionDelegate> DefaultLayout()
        {
            yield return SynopsisSection();
            yield return CommandUsageSection();
            yield return CommandArgumentsSection();
            yield return OptionsSection();
            yield return SubcommandsSection();
            yield return AdditionalArgumentsSection();
        }

        /// <summary>
        /// Writes a help section describing a command's synopsis.
        /// </summary>
        public static HelpSectionDelegate SynopsisSection() =>
            ctx =>
            {
                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpDescriptionTitle(), ctx.Command.Description, ctx.Output);
            };

        /// <summary>
        /// Writes a help section describing a command's usage.
        /// </summary>
        public static HelpSectionDelegate CommandUsageSection() =>
            ctx =>
            {
                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpUsageTitle(), ctx.HelpBuilder.GetUsage(ctx.Command), ctx.Output);
            };

        ///  <summary>
        /// Writes a help section describing a command's arguments.
        ///  </summary>
        public static HelpSectionDelegate CommandArgumentsSection() =>
            ctx =>
            {
                TwoColumnHelpRow[] commandArguments = ctx.HelpBuilder.GetCommandArgumentRows(ctx.Command, ctx).ToArray();

                if (commandArguments.Length <= 0)
                {
                    ctx.WasSectionSkipped = true;
                    return;
                }

                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpArgumentsTitle(), null, ctx.Output);
                ctx.HelpBuilder.WriteColumns(commandArguments, ctx);
            };

        ///  <summary>
        /// Writes a help section describing a command's options.
        ///  </summary>
        public static HelpSectionDelegate OptionsSection() =>
            ctx =>
            {
                var options = ctx.Command.Options.Where(x => !x.IsHidden).Select(x => ctx.HelpBuilder.GetTwoColumnRow(x, ctx)).ToArray();

                if (options.Length <= 0)
                {
                    ctx.WasSectionSkipped = true;
                    return;
                }

                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpOptionsTitle(), null, ctx.Output);
                ctx.HelpBuilder.WriteColumns(options, ctx);
                ctx.Output.WriteLine();
            };

        ///  <summary>
        /// Writes a help section describing a command's subcommands.
        ///  </summary>
        public static HelpSectionDelegate SubcommandsSection() =>
            ctx => ctx.HelpBuilder.WriteSubcommands(ctx);

        ///  <summary>
        /// Writes a help section describing a command's additional arguments, typically shown only when <see cref="Command.TreatUnmatchedTokensAsErrors"/> is set to <see langword="true"/>.
        ///  </summary>
        public static HelpSectionDelegate AdditionalArgumentsSection() =>
            ctx => ctx.HelpBuilder.WriteAdditionalArguments(ctx);

        /// <summary>
        /// Specifies custom help details for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to specify custom help details for.</param>
        /// <param name="firstColumnText">A delegate to display the first help column (typically name and usage information).</param>
        /// <param name="secondColumnText">A delegate to display second help column (typically the description).</param>
        /// <param name="defaultValue">A delegate to display the default value for the symbol.</param>
        public void CustomizeSymbol(ISymbol symbol,
            Func<HelpContext, string?>? firstColumnText = null,
            Func<HelpContext, string?>? secondColumnText = null,
            Func<HelpContext, string?>? defaultValue = null)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            _customizationsBySymbol ??= new();

            _customizationsBySymbol[symbol] = new Customization(firstColumnText, secondColumnText, defaultValue);
        }

        /// <summary>
        /// Customizes the help sections that will be displayed.
        /// </summary>
        /// <param name="getLayout">A delegate that returns the sections in the order in which they should be written.</param>
        public void CustomizeLayout(Func<HelpContext, IEnumerable<HelpSectionDelegate>> getLayout)
        {
            _getLayout = getLayout ?? throw new ArgumentNullException(nameof(getLayout));
        }

        private string GetUsage(ICommand command)
        {
            return string.Join(" ", GetUsageParts().Where(x => !string.IsNullOrWhiteSpace(x)));

            IEnumerable<string> GetUsageParts()
            {

                IEnumerable<ICommand> parentCommands =
                    command
                        .RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                        .Reverse();

                foreach (ICommand parentCommand in parentCommands)
                {
                    yield return parentCommand.Name;

                    yield return FormatArgumentUsage(parentCommand.Arguments);
                }

                var hasCommandWithHelp = command.Children
                    .OfType<ICommand>()
                    .Any(x => !x.IsHidden);

                if (hasCommandWithHelp)
                {
                    yield return LocalizationResources.HelpUsageCommandTitle();
                }

                var displayOptionTitle = command.Options.Any(x => !x.IsHidden);
                
                if (displayOptionTitle)
                {
                    yield return LocalizationResources.HelpUsageOptionsTitle();
                }

                if (!command.TreatUnmatchedTokensAsErrors)
                {
                    yield return LocalizationResources.HelpUsageAdditionalArguments();
                }
            }
        }

        private IEnumerable<TwoColumnHelpRow> GetCommandArgumentRows(ICommand command, HelpContext context) =>
            command
                .RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                .Reverse()
                .SelectMany(cmd => cmd.Arguments.Where(a => !a.IsHidden))
                .Select(a => GetTwoColumnRow(a, context))
                .Distinct();

        private void WriteSubcommands(HelpContext context)
        {
            var subcommands = context.Command.Children.OfType<ICommand>().Where(x => !x.IsHidden).Select(x => GetTwoColumnRow(x, context)).ToArray();

            if (subcommands.Length <= 0)
            {
                context.WasSectionSkipped = true;
                return;
            }

            WriteHeading(LocalizationResources.HelpCommandsTitle(), null, context.Output);
            WriteColumns(subcommands, context);
        }

        private void WriteAdditionalArguments(HelpContext context)
        {
            if (context.Command.TreatUnmatchedTokensAsErrors)
            {
                context.WasSectionSkipped = true;
                return;
            }

            WriteHeading(LocalizationResources.HelpAdditionalArgumentsTitle(),
                         LocalizationResources.HelpAdditionalArgumentsDescription(), context.Output);
        }

        private void WriteHeading(string? heading, string? description, TextWriter writer)
        {
            if (!string.IsNullOrWhiteSpace(heading))
            {
                writer.WriteLine(heading);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                int maxWidth = MaxWidth - Indent.Length;
                foreach (var part in WrapText(description!, maxWidth))
                {
                    writer.Write(Indent);
                    writer.WriteLine(part);
                }
            }
        }

        /// <summary>
        /// Writes the specified help rows, aligning output in columns.
        /// </summary>
        /// <param name="items">The help items to write out in columns.</param>
        /// <param name="context">The help context.</param>
        public void WriteColumns(IReadOnlyList<TwoColumnHelpRow> items, HelpContext context)
        {
            if (items.Count == 0)
            {
                return;
            }

            int windowWidth = MaxWidth;

            int firstColumnWidth = items.Select(x => x.FirstColumnText.Length).Max();
            int secondColumnWidth = items.Select(x => x.SecondColumnText.Length).Max();

            if (firstColumnWidth + secondColumnWidth + Indent.Length + Indent.Length > windowWidth)
            {
                int firstColumnMaxWidth = windowWidth / 2 - Indent.Length;
                if (firstColumnWidth > firstColumnMaxWidth)
                {
                    firstColumnWidth = items.SelectMany(x => WrapText(x.FirstColumnText, firstColumnMaxWidth).Select(x => x.Length)).Max();
                }
                secondColumnWidth = windowWidth - firstColumnWidth - Indent.Length - Indent.Length;
            }
            
            for (var i = 0; i < items.Count; i++)
            {
                var helpItem = items[i];
                IEnumerable<string> firstColumnParts = WrapText(helpItem.FirstColumnText, firstColumnWidth);
                IEnumerable<string> secondColumnParts = WrapText(helpItem.SecondColumnText, secondColumnWidth);

                foreach (var (first, second) in ZipWithEmpty(firstColumnParts, secondColumnParts))
                {
                    context.Output.Write($"{Indent}{first}");
                    if (!string.IsNullOrWhiteSpace(second))
                    {
                        int padSize = firstColumnWidth - first.Length;
                        string padding = "";
                        if (padSize > 0)
                        {
                            padding = new string(' ', padSize);
                        }

                        context.Output.Write($"{padding}{Indent}{second}");
                    }

                    context.Output.WriteLine();
                }
            }

            static IEnumerable<(string, string)> ZipWithEmpty(IEnumerable<string> first, IEnumerable<string> second)
            {
                using var enum1 = first.GetEnumerator();
                using var enum2 = second.GetEnumerator();
                bool hasFirst = false, hasSecond = false;
                while ((hasFirst = enum1.MoveNext()) | (hasSecond = enum2.MoveNext()))
                {
                    yield return (hasFirst ? enum1.Current : "", hasSecond ? enum2.Current : "");
                }
            }
        }

        private string FormatArgumentUsage(IReadOnlyList<IArgument> arguments)
        {
            var sb = StringBuilderPool.Default.Rent();

            try
            {
                var end = default(Stack<char>);

                for (var i = 0; i < arguments.Count; i++)
                {
                    var argument = arguments[i];
                    if (argument.IsHidden)
                    {
                        continue;
                    }

                    var arityIndicator =
                        argument.Arity.MaximumNumberOfValues > 1
                            ? "..."
                            : "";

                    var isOptional = IsOptional(argument);

                    if (isOptional)
                    {
                        sb.Append($"[<{argument.Name}>{arityIndicator}");
                        (end ??= new Stack<char>()).Push(']');
                    }
                    else
                    {
                        sb.Append($"<{argument.Name}>{arityIndicator}");
                    }

                    sb.Append(' ');
                }

                if (sb.Length > 0)
                {
                    sb.Length--;

                    if (end is { })
                    {
                        while (end.Count > 0)
                        {
                            sb.Append(end.Pop());
                        }
                    }
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Default.ReturnToPool(sb);
            }

            bool IsMultiParented(IArgument argument) =>
                argument is Argument a &&
                a.Parents.Count > 1;

            bool IsOptional(IArgument argument) =>
                IsMultiParented(argument) ||
                argument.Arity.MinimumNumberOfValues == 0;
        }

        private IEnumerable<HelpSectionDelegate> GetLayout(HelpContext context)
        {
            if (_getLayout is null)
            {
                _getLayout = _ => DefaultLayout();
            }
            return _getLayout(context);
        }

        private static IEnumerable<string> WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            //First handle existing new lines
            var parts = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string part in parts)
            {
                if (part.Length <= maxWidth)
                {
                    yield return part;
                }
                else
                {
                    //Long item, wrap it based on the width
                    for (int i = 0; i < part.Length;)
                    {
                        if (part.Length - i < maxWidth)
                        {
                            yield return part.Substring(i);
                            break;
                        }
                        else
                        {
                            int length = -1;
                            for (int j = 0; j + i < part.Length && j < maxWidth; j++)
                            {
                                if (char.IsWhiteSpace(part[i + j]))
                                {
                                    length = j + 1;
                                }
                            }
                            if (length == -1)
                            {
                                length = maxWidth;
                            }
                            yield return part.Substring(i, length);

                            i += length;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a help item for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <param name="context">The help context.</param>
        public TwoColumnHelpRow GetTwoColumnRow(
            ISymbol symbol,
            HelpContext context)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            Customization? customization = null;

            if (_customizationsBySymbol is { })
            {
                _customizationsBySymbol.TryGetValue(symbol, out customization);
            }

            if (symbol is IIdentifierSymbol identifierSymbol)
            {
                return GetIdentifierSymbolRow();
            }
            else if (symbol is IArgument argument)
            {
                return GetCommandArgumentRow(argument);
            }
            else
            {
                throw new NotSupportedException($"Symbol type {symbol.GetType()} is not supported.");
            }

            TwoColumnHelpRow GetIdentifierSymbolRow()
            {
                var firstColumnText =
                    customization?.GetFirstColumn?.Invoke(context) ??
                    GetIdentifierSymbolUsageTitle(identifierSymbol);

                var secondColumnText = 
                     customization?.GetSecondColumn?.Invoke(context) ??
                    GetIdentifierSymbolDescription(identifierSymbol, context);

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }

            TwoColumnHelpRow GetCommandArgumentRow(IArgument argument)
            {
                var firstColumnText =
                    customization?.GetFirstColumn?.Invoke(context) ??
                    GetArgumentUsageTitle(argument);

                var argumentDescription =
                    customization?.GetSecondColumn?.Invoke(context) ??
                    GetArgumentDescription(argument);

                var defaultValueDescription =
                    argument.HasDefaultValue
                        ? $"[{GetArgumentDefaultValueText(context.Command, argument, true, context)}]"
                        : "";

                var secondColumnText = $"{argumentDescription} {defaultValueDescription}".Trim();

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }
        }

        /// <summary>
        /// Gets the first column content for the specified symbol (typically the usage).
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <returns>Text to dispay.</returns>
        public string GetIdentifierSymbolUsageTitle(IIdentifierSymbol symbol)
        {
            var aliases = symbol.Aliases
                                .Select(r => r.SplitPrefix())
                                .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                                .GroupBy(t => t.Alias)
                                .Select(t => t.First())
                                .Select(t => $"{t.Prefix}{t.Alias}");

            var firstColumnText = string.Join(", ", aliases);

            foreach (var argument in symbol.Arguments())
            {
                if (!argument.IsHidden)
                {
                    var argumentFirstColumnText = GetArgumentUsageTitle(argument);

                    if (!string.IsNullOrWhiteSpace(argumentFirstColumnText))
                    {
                        firstColumnText += $" {argumentFirstColumnText}";
                    }
                }
            }

            if (symbol is IOption { IsRequired: true })
            {
                firstColumnText += $" {LocalizationResources.HelpOptionsRequired()}";
            }

            return firstColumnText;
        }

        /// <summary>
        /// Gets the second column content for the specified symbol (typically the description).
        /// </summary>
        /// <param name="symbol">The symbol to get the description for.</param>
        /// <param name="context">A parse result providing context for help formatting.</param>
        public string GetIdentifierSymbolDescription(IIdentifierSymbol symbol, HelpContext context)
        {
            return string.Join(" ", GetSecondColumnTextParts());

            IEnumerable<string> GetSecondColumnTextParts()
            {
                string? description = symbol.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }
                string argumentDescription = GetArgumentDescription();
                if (!string.IsNullOrWhiteSpace(argumentDescription))
                {
                    yield return argumentDescription;
                }
            }

            string GetArgumentDescription()
            {
                IEnumerable<IArgument> arguments = symbol.Arguments();
                var defaultArguments = arguments.Where(x => !x.IsHidden && x.HasDefaultValue).ToArray();

                if (defaultArguments.Length == 0) return "";

                var isSingleArgument = defaultArguments.Length == 1;
                var argumentDefaultValues = defaultArguments
                    .Select(argument => GetArgumentDefaultValueText(symbol, argument, isSingleArgument, context));
                return $"[{string.Join(", ", argumentDefaultValues)}]";
            }
        }

        private string GetArgumentDefaultValueText(
            IIdentifierSymbol parent,
            IArgument argument,
            bool displayArgumentName,
            HelpContext context)
        {
            string name = displayArgumentName ? LocalizationResources.HelpArgumentDefaultValueTitle() : argument.Name;
            if (_customizationsBySymbol is not null)
            {
                if (_customizationsBySymbol.TryGetValue(parent, out Customization customization) &&
                    customization.GetDefaultValue?.Invoke(context) is { } parentDefaultValue)
                {
                    return $"{name}: {parentDefaultValue}";
                }

                if (_customizationsBySymbol.TryGetValue(argument, out customization) &&
                    customization.GetDefaultValue?.Invoke(context) is { } ownDefaultValue)
                {
                    return $"{name}: {ownDefaultValue}";
                }
            }
            return $"{name}: {GetArgumentDefaultValue(argument)}";
        }

        /// <summary>
        /// Gets the argument default value to be displayed for help purposes.
        /// </summary>
        /// <param name="argument">The argument to get the description for.</param>
        public string GetArgumentDefaultValue(
            IArgument argument)
        {
            object? argumentDefaultValue = argument.GetDefaultValue();
            string? defaultValue;
            if (argumentDefaultValue is IEnumerable enumerable and not string)
            {
                defaultValue = string.Join("|", enumerable.OfType<object>().ToArray());
            }
            else
            {
                defaultValue = argumentDefaultValue?.ToString();
            }

            return defaultValue ?? string.Empty;
        }

        /// <summary>
        /// Gets the usage title for an argument (for example: <c>&lt;value&gt;</c>, typically used in the first column text in the arguments usage section, or within the synopsis.
        /// </summary>
        public string GetArgumentUsageTitle(IArgument argument)
        {
            if (argument.ValueType == typeof(bool) ||
                argument.ValueType == typeof(bool?))
            {
                if (argument.Parents.FirstOrDefault() is ICommand)
                {
                    return $"<{argument.Name}>";
                }
                else
                {
                    return "";
                }
            }

            string firstColumn;
            var completions = (argument is Argument a
                                   ? a.GetCompletions()
                                   : Array.Empty<CompletionItem>())
                .Select(item=>item.Label)
                .ToArray();

            var helpName = GetArgumentHelpName(argument);
            if (!string.IsNullOrEmpty(helpName))
            {
                firstColumn = helpName!;
            }
            else if (completions.Length > 0)
            {
                firstColumn = string.Join("|", completions);
            }
            else
            {
                firstColumn = argument.Name;
            }

            if (!string.IsNullOrWhiteSpace(firstColumn))
            {
                return $"<{firstColumn}>";
            }
            return firstColumn;
        }

        /// <summary>
        /// Gets the description for an argument, typically used in the second column text in the arguments usage section.
        /// </summary>
        public string GetArgumentDescription(IArgument argument) => argument.Description ?? string.Empty;

        private string? GetArgumentHelpName(IArgument argument)
        {
            var arg = argument as Argument;
            return arg?.HelpName;
        }

        private class Customization
        {
            public Customization(
                Func<HelpContext, string?>? getFirstColumn,
                Func<HelpContext, string?>? getSecondColumn,
                Func<HelpContext, string?>? getDefaultValue)
            {
                GetFirstColumn = getFirstColumn;
                GetSecondColumn = getSecondColumn;
                GetDefaultValue = getDefaultValue;
            }

            public Func<HelpContext, string?>? GetFirstColumn { get; }
            public Func<HelpContext, string?>? GetSecondColumn { get; }
            public Func<HelpContext, string?>? GetDefaultValue { get; }
        }
    }
}
