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

        public static string HelpView(this CommandDefinition commandDefinition)
        {
            if (commandDefinition == null)
            {
                throw new ArgumentNullException(nameof(commandDefinition));
            }

            var helpView = new StringBuilder();

            WriteSynopsis(commandDefinition, helpView);

            WriteArgumentsSection(commandDefinition, helpView);

            WriteOptionsSection(commandDefinition, helpView);

            WriteSubcommandsSection(commandDefinition, helpView);

            WriteAdditionalArgumentsSection(commandDefinition, helpView);

            return helpView.ToString();
        }

        private static void WriteAdditionalArgumentsSection(
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            if (commandDefinition?.TreatUnmatchedTokensAsErrors == true)
            {
                return;
            }

            helpView.Append(AdditionalArgumentsSection);
        }

        private static void WriteArgumentsSection(
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            var showArgHelp = commandDefinition.HasArguments && commandDefinition.HasHelp;
            var showParentArgHelp = false;

            if (commandDefinition.Parent != null)
            {
                showParentArgHelp = commandDefinition.Parent.HasArguments && commandDefinition.Parent.HasHelp;
            }

            if (!showArgHelp && !showParentArgHelp)
            {
                return;
            }

            var argHelp = commandDefinition.ArgumentDefinition?.Help;
            var parentArgHelp = commandDefinition.Parent?.ArgumentDefinition?.Help;

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
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            var options = commandDefinition
                .SymbolDefinitions
                .OfType<OptionDefinition>()
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
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            var subcommands = commandDefinition
                .SymbolDefinitions
                .OfType<CommandDefinition>()
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
            IReadOnlyCollection<SymbolDefinition> symbols,
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

        private static string LeftColumnText(SymbolDefinition symbolDefinition)
        {
            var builder = new StringBuilder();
            builder.Append(Indent);
            builder.Append(string.Join(", ", symbolDefinition.RawAliases.OrderBy(alias => alias.Length)));

            var argumentName = symbolDefinition.ArgumentDefinition?.Help?.Name;

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
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            helpView.Append(Synopsis.Title);

            var subcommands = commandDefinition
                .RecurseWhileNotNull(commandDef => commandDef.Parent)
                .Reverse();

            foreach (var subcommand in subcommands)
            {
                helpView.Append($" {subcommand.Name}");

                var argsName = subcommand.ArgumentDefinition?.Help?.Name;
                if (subcommand != commandDefinition && !string.IsNullOrWhiteSpace(argsName))
                {
                    helpView.Append($" <{argsName}>");
                }
            }

            var hasHelp = commandDefinition.SymbolDefinitions
                .Where(symbolDef => !(symbolDef is CommandDefinition))
                .Any(symbolDef => symbolDef.HasHelp);

            if (hasHelp)
            {
                helpView.Append(Synopsis.Options);
            }

            var argumentsName = commandDefinition.ArgumentDefinition?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                helpView.Append($" <{argumentsName}>");
            }

            if (commandDefinition.SymbolDefinitions.OfType<CommandDefinition>().Any())
            {
                helpView.Append(Synopsis.Command);
            }

            if (!commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                helpView.Append(Synopsis.AdditionalArguments);
            }

            helpView.AppendLine();
        }
    }
}
