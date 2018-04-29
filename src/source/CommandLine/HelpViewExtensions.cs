// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text;
using static System.Environment;
using static Microsoft.DotNet.Cli.CommandLine.DefaultHelpViewText;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class HelpViewExtensions
    {
        private static int columnGutterWidth = 3;

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
            if (command.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            helpView.Append(AdditionalArgumentsSection);
        }

        private static void WriteArgumentsSection(
            Command command,
            StringBuilder helpView)
        {
            var argName = command.ArgumentsRule.Help.Name;
            var argDescription = command.ArgumentsRule.Help.Description;

            var shouldWriteCommandArguments =
                !string.IsNullOrWhiteSpace(argName) &&
                !string.IsNullOrWhiteSpace(argDescription);

            var parentCommand = command.Parent;

            var parentArgName = (parentCommand?.ArgumentsRule).Help?.Name;
            var parentArgDescription = (parentCommand?.ArgumentsRule).Help?.Description;

            var shouldWriteParentCommandArguments =
                !string.IsNullOrWhiteSpace(parentArgName) &&
                !string.IsNullOrWhiteSpace(parentArgDescription);

            if (shouldWriteCommandArguments ||
                shouldWriteParentCommandArguments)
            {
                helpView.AppendLine();
                helpView.AppendLine(ArgumentsSection.Title);
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
            Command command,
            StringBuilder helpView)
        {
            var options = command
                .DefinedSymbols
                .Where(o => !(o is Command))
                .Where(o => !o.IsHidden())
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
                .DefinedSymbols
                .Where(o => !o.IsHidden())
                .OfType<Command>()
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
            Symbol[] symbols,
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

        private static string LeftColumnText(Symbol symbol)
        {
            var leftColumnText = "  " +
                                 string.Join(", ",
                                             symbol.RawAliases
                                                   .OrderBy(a => a.Length));

            var argumentName = symbol.ArgumentsRule.Help.Name;

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
            Command command,
            StringBuilder helpView)
        {
            helpView.Append(Synopsis.Title);

            foreach (var subcommand in command
                .RecurseWhileNotNull(c => c.Parent)
                .Reverse())
            {
                helpView.Append($" {subcommand.Name}");

                var argsName = subcommand.ArgumentsRule.Help.Name;
                if (subcommand != command &&
                    !string.IsNullOrWhiteSpace(argsName))
                {
                    helpView.Append($" <{argsName}>");
                }
            }

            if (command.DefinedSymbols
                       .Any(o => !(o is Command) &&
                                 !o.IsHidden()))
            {
                helpView.Append(Synopsis.Options);
            }

            var argumentsName = command.ArgumentsRule.Help.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                helpView.Append($" <{argumentsName}>");
            }

            if (command.DefinedSymbols.OfType<Command>().Any())
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