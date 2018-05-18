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
        private static int columnGutterWidth = 3;

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
            if (commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            helpView.Append(AdditionalArgumentsSection);
        }

        private static void WriteArgumentsSection(
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            var argName = commandDefinition.ArgumentDefinition.Help.Name;
            var argDescription = commandDefinition.ArgumentDefinition.Help.Description;

            var shouldWriteCommandArguments =
                !string.IsNullOrWhiteSpace(argName) &&
                !string.IsNullOrWhiteSpace(argDescription);

            var parentCommand = commandDefinition.Parent;

            var parentArgName = parentCommand?.ArgumentDefinition?.Help?.Name;
            var parentArgDescription = parentCommand?.ArgumentDefinition?.Help?.Description;

            var shouldWriteParentCommandArguments =
                !string.IsNullOrWhiteSpace(parentArgName) &&
                !string.IsNullOrWhiteSpace(parentArgDescription);

            if (shouldWriteCommandArguments ||
                shouldWriteParentCommandArguments)
            {
                helpView.AppendLine();
                helpView.AppendLine(DefaultHelpViewText.ArgumentsSection.Title);
            }
            else
            {
                return;
            }

            var indent = "  ";
            var argLeftColumnText = $"{indent}<{argName}>";
            var parentArgLeftColumnText = $"{indent}<{parentArgName}>";
            var leftColumnWidth =
                Math.Max(argLeftColumnText.Length,
                         parentArgLeftColumnText.Length) +
                columnGutterWidth;

            if (shouldWriteParentCommandArguments)
            {
                WriteColumnizedSummary(
                    parentArgLeftColumnText,
                    parentArgDescription,
                    leftColumnWidth,
                    helpView);
            }

            if (shouldWriteCommandArguments)
            {
                WriteColumnizedSummary(
                    argLeftColumnText,
                    argDescription,
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
                .Where(o => !(o is CommandDefinition))
                .Where(o => !o.IsHidden())
                .ToArray();

            if (!options.Any())
            {
                return;
            }

            helpView.AppendLine();
            helpView.AppendLine(DefaultHelpViewText.OptionsSection.Title);

            WriteOptionsList(options, helpView);
        }

        private static void WriteSubcommandsSection(
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            var subcommands = commandDefinition
                .SymbolDefinitions
                .Where(o => !o.IsHidden())
                .OfType<CommandDefinition>()
                .ToArray();

            if (!subcommands.Any())
            {
                return;
            }

            helpView.AppendLine();
            helpView.AppendLine(DefaultHelpViewText.CommandsSection.Title);

            WriteOptionsList(subcommands, helpView);
        }

        private static void WriteOptionsList(
            IReadOnlyCollection<SymbolDefinition> symbols,
            StringBuilder helpView)
        {
            var leftColumnTextFor = symbols
                .ToDictionary(o => o, LeftColumnText);

            var leftColumnWidth = leftColumnTextFor
                                      .Values
                                      .Select(s => s.Length)
                                      .OrderBy(length => length)
                                      .Last() + columnGutterWidth;

            foreach (var symbol in symbols)
            {
                WriteColumnizedSummary(leftColumnTextFor[symbol],
                                       symbol.Description,
                                       leftColumnWidth,
                                       helpView);
            }
        }

        private static string LeftColumnText(SymbolDefinition symbolDefinition)
        {
            var leftColumnText = "  " +
                                 string.Join(", ",
                                             symbolDefinition.RawAliases
                                                   .OrderBy(a => a.Length));

            var argumentName = symbolDefinition.ArgumentDefinition.Help.Name;

            if (!string.IsNullOrWhiteSpace(argumentName))
            {
                leftColumnText += $" <{argumentName}>";
            }

            return leftColumnText;
        }

        private static void WriteColumnizedSummary(
            string leftColumnText,
            string rightColumnText,
            int width,
            StringBuilder helpView)
        {
            helpView.Append(leftColumnText);

            if (leftColumnText.Length <= width - 2)
            {
                helpView.Append(new string(' ', width - leftColumnText.Length));
            }
            else
            {
                helpView.AppendLine();
                helpView.Append(new string(' ', width));
            }

            var descriptionWithLineWraps = string.Join(
                NewLine + new string(' ', width),
                rightColumnText
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()));

            helpView.AppendLine(descriptionWithLineWraps);
        }

        private static void WriteSynopsis(
            CommandDefinition commandDefinition,
            StringBuilder helpView)
        {
            helpView.Append(DefaultHelpViewText.Synopsis.Title);

            foreach (var subcommand in commandDefinition
                .RecurseWhileNotNull(c => c.Parent)
                .Reverse())
            {
                helpView.Append($" {subcommand.Name}");

                var argsName = subcommand.ArgumentDefinition.Help.Name;
                if (subcommand != commandDefinition &&
                    !string.IsNullOrWhiteSpace(argsName))
                {
                    helpView.Append($" <{argsName}>");
                }
            }

            if (commandDefinition.SymbolDefinitions
                       .Any(o => !(o is CommandDefinition) &&
                                 !o.IsHidden()))
            {
                helpView.Append(DefaultHelpViewText.Synopsis.Options);
            }

            var argumentsName = commandDefinition.ArgumentDefinition.Help.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                helpView.Append($" <{argumentsName}>");
            }

            if (commandDefinition.SymbolDefinitions.OfType<CommandDefinition>().Any())
            {
                helpView.Append(DefaultHelpViewText.Synopsis.Command);
            }

            if (!commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                helpView.Append(DefaultHelpViewText.Synopsis.AdditionalArguments);
            }

            helpView.AppendLine();
        }
    }
}
