// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Formats output to be shown to users to describe how to use a command line tool.
    /// </summary>
    public partial class HelpBuilder 
    {
        private const string Indent = "  ";

        private Dictionary<CliSymbol, Customization>? _customizationsBySymbol;
        private Func<HelpContext, IEnumerable<Action<HelpContext>>>? _getLayout;

        /// <param name="maxWidth">The maximum width in characters after which help output is wrapped.</param>
        public HelpBuilder(int maxWidth = int.MaxValue)
        {
            if (maxWidth <= 0)
            {
                maxWidth = int.MaxValue;
            }
            MaxWidth = maxWidth;
        }

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

            if (context.Command.Hidden)
            {
                return;
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

        /// <summary>
        /// Specifies custom help details for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to specify custom help details for.</param>
        /// <param name="firstColumnText">A delegate to display the first help column (typically name and usage information).</param>
        /// <param name="secondColumnText">A delegate to display second help column (typically the description).</param>
        /// <param name="defaultValue">A delegate to display the default value for the symbol.</param>
        public void CustomizeSymbol(CliSymbol symbol,
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
        public void CustomizeLayout(Func<HelpContext, IEnumerable<Action<HelpContext>>> getLayout)
        {
            _getLayout = getLayout ?? throw new ArgumentNullException(nameof(getLayout));
        }

        private string GetUsage(CliCommand command)
        {
            return string.Join(" ", GetUsageParts().Where(x => !string.IsNullOrWhiteSpace(x)));

            IEnumerable<string> GetUsageParts()
            {
                bool displayOptionTitle = false;

                IEnumerable<CliCommand> parentCommands =
                    command
                        .RecurseWhileNotNull(c => c.Parents.OfType<CliCommand>().FirstOrDefault())
                        .Reverse();

                foreach (var parentCommand in parentCommands)
                {
                    if (!displayOptionTitle)
                    {
                        displayOptionTitle = parentCommand.HasOptions && parentCommand.Options.Any(x => x.Recursive && !x.Hidden);
                    }

                    yield return parentCommand.Name;

                    if (parentCommand.HasArguments)
                    {
                        yield return FormatArgumentUsage(parentCommand.Arguments);
                    }
                }

                var hasCommandWithHelp = command.HasSubcommands && command.Subcommands.Any(x => !x.Hidden);

                if (hasCommandWithHelp)
                {
                    yield return LocalizationResources.HelpUsageCommand();
                }

                displayOptionTitle = displayOptionTitle || (command.HasOptions && command.Options.Any(x => !x.Hidden));
                
                if (displayOptionTitle)
                {
                    yield return LocalizationResources.HelpUsageOptions();
                }

                if (!command.TreatUnmatchedTokensAsErrors)
                {
                    yield return LocalizationResources.HelpUsageAdditionalArguments();
                }
            }
        }

        private IEnumerable<TwoColumnHelpRow> GetCommandArgumentRows(CliCommand command, HelpContext context) =>
            command
                .RecurseWhileNotNull(c => c.Parents.OfType<CliCommand>().FirstOrDefault())
                .Reverse()
                .SelectMany(cmd => cmd.Arguments.Where(a => !a.Hidden))
                .Select(a => GetTwoColumnRow(a, context))
                .Distinct();

        private void WriteSubcommands(HelpContext context)
        {
            var subcommands = context.Command.Subcommands.Where(x => !x.Hidden).Select(x => GetTwoColumnRow(x, context)).ToArray();

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

        private string FormatArgumentUsage(IList<CliArgument> arguments)
        {
            var sb = new StringBuilder(arguments.Count * 100);

            var end = default(List<char>);

            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                if (argument.Hidden)
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
                    (end ??= new ()).Add(']');
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
                        sb.Append(end[end.Count - 1]);
                        end.RemoveAt(end.Count - 1);
                    }
                }
            }

            return sb.ToString();
            
            bool IsOptional(CliArgument argument) =>
                argument.Arity.MinimumNumberOfValues == 0;
        }

        private IEnumerable<Action<HelpContext>> GetLayout(HelpContext context)
        {
            if (_getLayout is null)
            {
                _getLayout = _ => Default.GetLayout();
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
            CliSymbol symbol,
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

            if (symbol is CliOption or CliCommand)
            {
                return GetOptionOrCommandRow();
            }
            else if (symbol is CliArgument argument)
            {
                return GetCommandArgumentRow(argument);
            }
            else
            {
                throw new NotSupportedException($"Symbol type {symbol.GetType()} is not supported.");
            }

            TwoColumnHelpRow GetOptionOrCommandRow()
            {
                var firstColumnText = customization?.GetFirstColumn?.Invoke(context) 
                    ?? (symbol is CliOption option
                            ? Default.GetOptionUsageLabel(option)
                            : Default.GetCommandUsageLabel((CliCommand)symbol));

                var customizedSymbolDescription = customization?.GetSecondColumn?.Invoke(context);

                var symbolDescription = customizedSymbolDescription ?? symbol.Description ?? string.Empty;

                //in case symbol description is customized, do not output default value
                //default value output is not customizable for identifier symbols
                var defaultValueDescription = customizedSymbolDescription == null
                    ? GetSymbolDefaultValue(symbol)
                    : string.Empty;

                var secondColumnText = $"{symbolDescription} {defaultValueDescription}".Trim();

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }

            TwoColumnHelpRow GetCommandArgumentRow(CliArgument argument)
            {
                var firstColumnText =
                    customization?.GetFirstColumn?.Invoke(context) ?? Default.GetArgumentUsageLabel(argument);

                var argumentDescription =
                    customization?.GetSecondColumn?.Invoke(context) ?? Default.GetArgumentDescription(argument);

                var defaultValueDescription =
                    argument.HasDefaultValue
                        ? $"[{GetArgumentDefaultValue(context.Command, argument, true, context)}]"
                        : "";

                var secondColumnText = $"{argumentDescription} {defaultValueDescription}".Trim();

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }

            string GetSymbolDefaultValue(CliSymbol symbol)
            {
                IList<CliArgument> arguments = symbol.Arguments();
                var defaultArguments = arguments.Where(x => !x.Hidden && x.HasDefaultValue).ToArray();

                if (defaultArguments.Length == 0) return "";

                var isSingleArgument = defaultArguments.Length == 1;
                var argumentDefaultValues = defaultArguments
                    .Select(argument => GetArgumentDefaultValue(symbol, argument, isSingleArgument, context));
                return $"[{string.Join(", ", argumentDefaultValues)}]";
            }
        }

        private string GetArgumentDefaultValue(
            CliSymbol parent,
            CliArgument argument,
            bool displayArgumentName,
            HelpContext context)
        {
            string label = displayArgumentName 
                              ? LocalizationResources.HelpArgumentDefaultValueLabel() 
                              : argument.Name;

            string? displayedDefaultValue = null;

            if (_customizationsBySymbol is not null)
            {
                if (_customizationsBySymbol.TryGetValue(parent, out var customization) &&
                    customization.GetDefaultValue?.Invoke(context) is { } parentDefaultValue)
                {
                    displayedDefaultValue = parentDefaultValue;
                }
                else if (_customizationsBySymbol.TryGetValue(argument, out customization) &&
                         customization.GetDefaultValue?.Invoke(context) is { } ownDefaultValue)
                {
                    displayedDefaultValue = ownDefaultValue;
                }
            }

            displayedDefaultValue ??= Default.GetArgumentDefaultValue(argument);

            if (string.IsNullOrWhiteSpace(displayedDefaultValue))
            {
                return "";
            }

            return $"{label}: {displayedDefaultValue}";
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
