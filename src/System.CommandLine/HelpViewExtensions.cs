// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Environment;
using static System.CommandLine.DefaultHelpViewText;

namespace System.CommandLine
{
    public static class HelpViewExtensions
    {
        private const int MaxWidthLeeWay = 2;
        private const int ColumnGutterWidth = 3;
        private const string Indent = "  ";

        public static string HelpView(this Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var helpView = new StringBuilder();

            WriteSynopsis(command, helpView);

            WriteArgumentsSection(command, helpView);

            WriteOptionsSection(command, helpView);

            WriteSubcommandsSection(command, helpView);

            WriteAdditionalArgumentsSection(command, helpView);

            return helpView.ToString();
        }

        private static void WriteAdditionalArgumentsSection(
            Command command,
            StringBuilder helpView)
        {
            if (command?.TreatUnmatchedTokensAsErrors == true)
            {
                return;
            }

            helpView.Append(AdditionalArgumentsSection);
        }

        private static void WriteArgumentsSection(
            Command command,
            StringBuilder helpView)
        {
            var showArgHelp = command.HasArguments && command.HasHelp;
            var showParentArgHelp = false;

            if (command.Parent != null)
            {
                showParentArgHelp = command.Parent.HasArguments && command.Parent.HasHelp;
            }

            if (!showArgHelp && !showParentArgHelp)
            {
                return;
            }

            var argHelp = command.Argument?.Help;
            var parentArgHelp = command.Parent?.Argument?.Help;

            helpView?.AppendLine();
            helpView?.AppendLine(ArgumentsSection.Title);

            var argLeftColumnText = showArgHelp ? $"{Indent}<{argHelp?.Name}>" : "";
            var parentArgLeftColumnText = showParentArgHelp ? $"{Indent}<{parentArgHelp?.Name}>" : "";
            var leftColumnWidth = ColumnGutterWidth + Math.Max(argLeftColumnText.Length, parentArgLeftColumnText.Length);

            if (showParentArgHelp)
            {
                WriteColumnizedSummary(
                    parentArgLeftColumnText,
                    parentArgHelp?.Description,
                    leftColumnWidth,
                    helpView);
            }

            if (showArgHelp)
            {
                WriteColumnizedSummary(
                    argLeftColumnText,
                    argHelp?.Description,
                    leftColumnWidth,
                    helpView);
            }
        }

        private static void WriteOptionsSection(
            Command command,
            StringBuilder helpView)
        {
            var options = command
                .Symbols
                .OfType<Option>()
                .Where(opt => opt.HasHelp)
                .ToArray();

            if (!options.Any())
            {
                return;
            }

            helpView.AppendLine();
            helpView.AppendLine(OptionsSection.Title);

            WriteOptionsList(options, helpView);
        }

        private static void WriteSubcommandsSection(
            Command command,
            StringBuilder helpView)
        {
            var subcommands = command
                .Symbols
                .OfType<Command>()
                .Where(subCommand => subCommand.HasHelp)
                .ToArray();

            if (!subcommands.Any())
            {
                return;
            }

            helpView.AppendLine();
            helpView.AppendLine(CommandsSection.Title);

            WriteOptionsList(subcommands, helpView);
        }

        private static void WriteOptionsList(
            IReadOnlyCollection<Symbol> symbols,
            StringBuilder helpView)
        {
            var leftColumnTextFor = symbols.ToDictionary(symbol => symbol, LeftColumnText);

             var maxWidth = leftColumnTextFor
                .Values
                .Select(symbol => symbol.Length)
                .OrderByDescending(length => length)
                .First();

            var leftColumnWidth = ColumnGutterWidth + maxWidth;

            foreach (var symbol in symbols)
            {
                WriteColumnizedSummary(
                    leftColumnTextFor[symbol],
                    symbol.Description,
                    leftColumnWidth,
                    helpView);
            }
        }

        private static string LeftColumnText(Symbol symbol)
        {
            var builder = new StringBuilder();
            builder.Append(Indent);
            builder.Append(string.Join(", ", symbol.RawAliases.OrderBy(alias => alias.Length)));

            var argumentName = symbol.Argument?.Help?.Name;

            if (!string.IsNullOrWhiteSpace(argumentName))
            {
                builder.Append($" <{argumentName}>");
            }

            return builder.ToString();
        }

        private static void WriteColumnizedSummary(
            string leftColumnText,
            string rightColumnText,
            int maxWidth,
            StringBuilder helpView)
        {
            if (leftColumnText == null)
            {
                leftColumnText = "";
            }

            if (rightColumnText == null)
            {
                rightColumnText = "";
            }

            helpView.Append(leftColumnText);

            if (leftColumnText.Length <= maxWidth - MaxWidthLeeWay)
            {
                helpView.Append(new string(' ', maxWidth - leftColumnText.Length));
            }
            else
            {
                helpView.AppendLine();
                helpView.Append(new string(' ', maxWidth));
            }

            var descriptionWithLineWraps = string.Join(
                NewLine + new string(' ', maxWidth),
                rightColumnText
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()));

            helpView.AppendLine(descriptionWithLineWraps);
        }

        private static void WriteSynopsis(
            Command command,
            StringBuilder helpView)
        {
            helpView.Append(Synopsis.Title);

            var subcommands = command
                .RecurseWhileNotNull(commandDef => commandDef.Parent)
                .Reverse();

            foreach (var subcommand in subcommands)
            {
                helpView.Append($" {subcommand.Name}");

                var argsName = subcommand.Argument?.Help?.Name;
                if (subcommand != command && !string.IsNullOrWhiteSpace(argsName))
                {
                    helpView.Append($" <{argsName}>");
                }
            }

            var hasHelp = command.Symbols
                .Where(symbolDef => !(symbolDef is Command))
                .Any(symbolDef => symbolDef.HasHelp);

            if (hasHelp)
            {
                helpView.Append(Synopsis.Options);
            }

            var argumentsName = command.Argument?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                helpView.Append($" <{argumentsName}>");
            }

            if (command.Symbols.OfType<Command>().Any())
            {
                helpView.Append(Synopsis.Command);
            }

            if (!command.TreatUnmatchedTokensAsErrors)
            {
                helpView.Append(Synopsis.AdditionalArguments);
            }

            helpView.AppendLine();
        }
    }
}
