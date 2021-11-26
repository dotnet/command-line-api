// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace System.CommandLine.Help
{
    /// <inheritdoc />
    public class HelpBuilder : IHelpBuilder
    {
        private const string Indent = "  ";

        private Dictionary<ISymbol, Customization>? _customizationsBySymbol;

        /// <param name="localizationResources">Resources used to localize the help output.</param>
        /// <param name="maxWidth">The maximum width in characters after which help output is wrapped.</param>
        public HelpBuilder(LocalizationResources localizationResources, int maxWidth = int.MaxValue)
        {
            LocalizationResources = localizationResources ?? throw new ArgumentNullException(nameof(localizationResources));
            if (maxWidth <= 0) maxWidth = int.MaxValue;
            MaxWidth = maxWidth;
        }

        /// <summary>
        /// Provides localizable strings for help and error messages.
        /// </summary>
        protected LocalizationResources LocalizationResources { get; }

        /// <summary>
        /// The maximum width for which to format help output.
        /// </summary>
        public int MaxWidth { get; }

        /// <summary>
        /// Writes help for the specified command.
        /// </summary>
        /// <param name="command">The command to write help output for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        public virtual void Write(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (parseResult is null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            if (command.IsHidden)
            {
                return;
            }

            var context = new HelpContext(this, parseResult, command);

            foreach (var section in DefaultLayout())
            {
                switch (section)
                {
                    case HelpDelegate d:
                        var output = d(context);
                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            writer.WriteLine(output);
                        }

                        break;

                    default:
                        writer.WriteLine(section);
                        break;
                }
            }
        }

        private IEnumerable<object> DefaultLayout()
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
        public static HelpDelegate SynopsisSection() =>
            ctx =>
            {
                using var output = new StringWriter();
                ctx.HelpBuilder.WriteSynopsis(ctx.Command, output);
                return output.ToString();
            };

        /// <summary>
        /// Writes a help section describing a command's usage.
        /// </summary>
        public static HelpDelegate CommandUsageSection() =>
            ctx =>
            {
                var output = new StringWriter();
                ctx.HelpBuilder.WriteCommandUsage(ctx.Command, output);
                return output.ToString();
            };

        ///  <summary>
        /// Writes a help section describing a command's arguments.
        ///  </summary>
        public static HelpDelegate CommandArgumentsSection() =>
            ctx =>
            {
                var output = new StringWriter();
                ctx.HelpBuilder.WriteCommandArguments(ctx.Command, output, ctx.ParseResult);
                return output.ToString();
            };

        ///  <summary>
        /// Writes a help section describing a command's options.
        ///  </summary>
        public static HelpDelegate OptionsSection() =>
            ctx =>
            {
                var output = new StringWriter();
                ctx.HelpBuilder.WriteOptions(ctx.Command, output, ctx.ParseResult);
                return output.ToString();
            };

        ///  <summary>
        /// Writes a help section describing a command's subcommands.
        ///  </summary>
        public static HelpDelegate SubcommandsSection() =>
            ctx =>
            {
                var output = new StringWriter();
                ctx.HelpBuilder.WriteSubcommands(ctx.Command, output, ctx.ParseResult);
                return output.ToString();
            };

        ///  <summary>
        /// Writes a help section describing a command's additional arguments, typically shown only when <see cref="Command.TreatUnmatchedTokensAsErrors"/> is set to <see langword="true"/>.
        ///  </summary>
        public static HelpDelegate AdditionalArgumentsSection() =>
            ctx =>
            {
                var output = new StringWriter();
                ctx.HelpBuilder.WriteAdditionalArguments(ctx.Command, output);
                return output.ToString();
            };

        /// <summary>
        /// Specifies custom help details for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to specify custom help details for.</param>
        /// <param name="firstColumnText">A delegate to display the first help column (typically name and usage information).</param>
        /// <param name="secondColumnText">A delegate to display second help column (typically the description).</param>
        /// <param name="defaultValue">A delegate to display the default value for the symbol.</param>
        internal void Customize(ISymbol symbol,
            Func<ParseResult?, string?>? firstColumnText = null,
            Func<ParseResult?, string?>? secondColumnText = null,
            Func<ParseResult?, string?>? defaultValue = null)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            _customizationsBySymbol ??= new();

            _customizationsBySymbol[symbol] = new Customization(firstColumnText, secondColumnText, defaultValue);
        }

        /// <summary>
        /// Writes the synopsis for the specified command.
        /// </summary>
        /// <param name="command">The command to write help details for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        private void WriteSynopsis(ICommand command, TextWriter writer)
        {
            WriteHeading(LocalizationResources.HelpDescriptionTitle(), command.Description, writer);
        }

        /// <summary>
        /// Writes usage for the specified command.
        /// </summary>
        /// <param name="command">The command to write help details for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        private void WriteCommandUsage(ICommand command, TextWriter writer)
        {
            string description = GetUsage(command);
            WriteHeading(LocalizationResources.HelpUsageTitle(), description, writer);
        }

        /// <summary>
        /// Gets the usage for the specified command.
        /// </summary>
        /// <param name="command">The command to get usage for.</param>
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

        /// <summary>
        /// Writes help output for the specified command's arguments.
        /// </summary>
        /// <param name="command">The command to write out argument help for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        private void WriteCommandArguments(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            TwoColumnHelpRow[] commandArguments = GetCommandArgumentRows(command, parseResult).ToArray();

            if (commandArguments.Length > 0)
            {
                WriteHeading(LocalizationResources.HelpArgumentsTitle(), null, writer);
                RenderAsColumns(writer, commandArguments);
            }
        }

        /// <summary>
        /// Gets help items for the specified command's arguments.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        /// <returns>Help items for the specified command's arguments.</returns>
        private IEnumerable<TwoColumnHelpRow> GetCommandArgumentRows(ICommand command, ParseResult parseResult)
        {
            return command.RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                    .Reverse()
                    .SelectMany(cmd => GetArguments(cmd, parseResult))
                    .Distinct();

            IEnumerable<TwoColumnHelpRow> GetArguments(ICommand command, ParseResult parseResult)
            {
                var arguments = command.Arguments.Where(x => !x.IsHidden);

                foreach (IArgument argument in arguments)
                {
                    string argumentFirstColumn = GetArgumentFirstColumnText(argument, parseResult);

                    yield return new TwoColumnHelpRow(argumentFirstColumn, string.Join(" ", GetArgumentSecondColumnText(command, argument)));
                }
            }

            IEnumerable<string> GetArgumentSecondColumnText(IIdentifierSymbol parent, IArgument argument)
            {
                string? description = argument.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }

                if (argument.HasDefaultValue)
                {
                    yield return $"[{GetArgumentDefaultValueText(parent, argument, true, parseResult)}]";
                }
            }
        }

        /// <summary>
        /// Writes help output for the specified command's options.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        /// <param name="writer">The writer to write help output to.</param>
        private void WriteOptions(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            var options = GetOptionRows(command, parseResult).ToArray();

            if (options.Length > 0)
            {
                WriteHeading(LocalizationResources.HelpOptionsTitle(), null, writer);
                RenderAsColumns(writer, options);
            }
        }

        /// <summary>
        /// Gets help rows for the specified command's options.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        private IEnumerable<TwoColumnHelpRow> GetOptionRows(ICommand command, ParseResult parseResult)
            => command.Options.Where(x => !x.IsHidden).Select(x => GetTwoColumnRow(x, parseResult));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        /// <param name="writer">The writer to write help output to.</param>
        private void WriteSubcommands(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            var subcommands = GetSubcommandRows(command, parseResult).ToArray();

            if (subcommands.Length > 0)
            {
                WriteHeading(LocalizationResources.HelpCommandsTitle(), null, writer);
                RenderAsColumns(writer, subcommands);
            }
        }

        /// <summary>
        /// Gets help items for the specified command's subcommands.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        private IEnumerable<TwoColumnHelpRow> GetSubcommandRows(ICommand command, ParseResult parseResult)
            => command.Children.OfType<ICommand>().Where(x => !x.IsHidden).Select(x => GetTwoColumnRow(x, parseResult));

        /// <summary>
        /// Writes help output for additional arguments.
        /// </summary>
        private void WriteAdditionalArguments(ICommand command, TextWriter writer)
        {
            if (command.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            WriteHeading(LocalizationResources.HelpAdditionalArgumentsTitle(),
                LocalizationResources.HelpAdditionalArgumentsDescription(), writer);
        }

        /// <summary>
        /// Writes a heading to help output.
        /// </summary>
        /// <param name="heading">The heading text.</param>
        /// <param name="description">The description text, which will typically be indented and, if it's long enough, wrapped.</param>
        /// <param name="writer">The writer to write help output to.</param>
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

        /// <summary>
        /// Writes the specified help items, aligning output in columns.
        /// </summary>
        /// <param name="writer">The writer to write help output to.</param>
        /// <param name="items">The help items to write out in columns.</param>
        private void RenderAsColumns(TextWriter writer, params TwoColumnHelpRow[] items)
        {
            if (items.Length == 0)
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

            foreach (var helpItem in items)
            {
                IEnumerable<string> firstColumnParts = WrapText(helpItem.FirstColumnText, firstColumnWidth);
                IEnumerable<string> secondColumnParts = WrapText(helpItem.SecondColumnText, secondColumnWidth);

                foreach (var (first, second) in ZipWithEmpty(firstColumnParts, secondColumnParts))
                {
                    writer.Write($"{Indent}{first}");
                    if (!string.IsNullOrWhiteSpace(second))
                    {
                        int padSize = firstColumnWidth - first.Length;
                        string padding = "";
                        if (padSize > 0)
                        {
                            padding = new string(' ', padSize);
                        }
                        writer.Write($"{padding}{Indent}{second}");
                    }
                    writer.WriteLine();
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
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        private TwoColumnHelpRow GetTwoColumnRow(IIdentifierSymbol symbol, ParseResult parseResult)
        {
            string firstColumnText;
            if (_customizationsBySymbol is { } &&
                _customizationsBySymbol.TryGetValue(symbol, out Customization customization) &&
                customization.GetFirstColumn?.Invoke(parseResult) is { } firstColumn)
            {
                firstColumnText = firstColumn;
            }
            else
            {
                var rawAliases = symbol.Aliases
                                 .Select(r => r.SplitPrefix())
                                 .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                                 .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                                 .GroupBy(t => t.Alias)
                                 .Select(t => t.First())
                                 .Select(t => $"{t.Prefix}{t.Alias}");

                firstColumnText = string.Join(", ", rawAliases);

                foreach (var argument in symbol.Arguments())
                {
                    if (!argument.IsHidden)
                    {
                        var argumentFirstColumn = GetArgumentFirstColumnText(argument, parseResult);
                        if (!string.IsNullOrWhiteSpace(argumentFirstColumn))
                        {
                            firstColumnText += $" {argumentFirstColumn}";
                        }
                    }
                }

                if (symbol is IOption { IsRequired: true })
                {
                    firstColumnText += $" {LocalizationResources.HelpOptionsRequired()}";
                }
            }

            return new TwoColumnHelpRow(firstColumnText, GetSecondColumnText(symbol, parseResult));
        }

        /// <summary>
        /// Gets the second column content for the specified symbol (typically the description).
        /// </summary>
        /// <param name="symbol">The symbol to get the description for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        private string GetSecondColumnText(IIdentifierSymbol symbol, ParseResult parseResult)
        {
            return string.Join(" ", GetSecondColumnTextParts());

            IEnumerable<string> GetSecondColumnTextParts()
            {
                string? description = symbol.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }
                else if (
                    _customizationsBySymbol is { } &&
                    _customizationsBySymbol.TryGetValue(symbol, out var customization) &&
                    customization.GetSecondColumn?.Invoke(parseResult) is { } descriptionValue)
                {
                    yield return descriptionValue;
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
                    .Select(argument => GetArgumentDefaultValueText(symbol, argument, isSingleArgument, parseResult));
                return $"[{string.Join(", ", argumentDefaultValues)}]";
            }
        }

        private string GetArgumentDefaultValueText(IIdentifierSymbol parent, IArgument argument, bool displayArgumentName, ParseResult parseResult)
        {
            string? defaultValue;
            if (_customizationsBySymbol is { } &&
                _customizationsBySymbol.TryGetValue(parent, out Customization customization) &&
                customization.GetDefaultValue?.Invoke(parseResult) is { } parentSetDefaultValue)
            {
                defaultValue = parentSetDefaultValue;
            }
            else if (_customizationsBySymbol is { } && _customizationsBySymbol.TryGetValue(argument, out customization) &&
                     customization.GetDefaultValue?.Invoke(parseResult) is { } setDefaultValue)
            {
                defaultValue = setDefaultValue;
            }
            else
            {
                object? argumentDefaultValue = argument.GetDefaultValue();
                if (argumentDefaultValue is IEnumerable enumerable && !(argumentDefaultValue is string))
                {
                    defaultValue = string.Join("|", enumerable.OfType<object>().ToArray());
                }
                else
                {
                    defaultValue = argumentDefaultValue?.ToString();
                }
            }

            string name = displayArgumentName ?
                LocalizationResources.HelpArgumentDefaultValueTitle() :
                argument.Name;

            return $"{name}: {defaultValue}";
        }

        /// <summary>
        /// Gets the first column text for the specified argument (typically the name and usage information).
        /// </summary>
        /// <param name="argument">The argument to get the first column text for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        private string GetArgumentFirstColumnText(IArgument argument, ParseResult parseResult)
        {
            if (_customizationsBySymbol is { } &&
                _customizationsBySymbol.TryGetValue(argument, out Customization customization) &&
                customization.GetFirstColumn?.Invoke(parseResult) is { } firstColumnText)
            {
                return firstColumnText;
            }

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
            var suggestions = argument.GetSuggestions().ToArray();
            var helpName = GetArgumentHelpName(argument);
            if (!string.IsNullOrEmpty(helpName))
            {
                firstColumn = helpName!;
            }
            else if (suggestions.Length > 0)
            {
                firstColumn = string.Join("|", suggestions);
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

        private string? GetArgumentHelpName(IArgument argument)
        {
            var arg = argument as Argument;
            return arg?.HelpName;
        }

        private class Customization
        {
            public Customization(
                Func<ParseResult?, string?>? getFirstColumn,
                Func<ParseResult?, string?>? getSecondColumn,
                Func<ParseResult?, string?>? getDefaultValue)
            {
                GetFirstColumn = getFirstColumn;
                GetSecondColumn = getSecondColumn;
                GetDefaultValue = getDefaultValue;
            }

            public Func<ParseResult?, string?>? GetFirstColumn { get; }
            public Func<ParseResult?, string?>? GetSecondColumn { get; }
            public Func<ParseResult?, string?>? GetDefaultValue { get; }
        }
    }
}
