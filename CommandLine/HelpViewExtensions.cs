using System;
using System.Linq;
using System.Text;
using static System.Environment;
using static Microsoft.DotNet.Cli.CommandLine.DefaultHelpViewText;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class HelpViewExtensions
    {
        internal static string HelpView(this Parser parser) =>
            parser.DefinedOptions
                  .FlattenBreadthFirst()
                  .OfType<Command>()
                  .FirstOrDefault()
                  ?.HelpView() ??
            parser.DefinedOptions
                  .First()
                  .HelpText;

        public static string HelpView(this Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var helpView = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(command.HelpText))
            {
                helpView.AppendLine(command.HelpText);
                helpView.AppendLine();
            }

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
            var name = command.ArgumentsRule.Name;
            var description = command.ArgumentsRule.Description;

            var shouldWriteCommandArguments =
                !string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(description);

            var parentCommand = command.Parent as Command;

            var parentArgName = parentCommand?.ArgumentsRule?.Name;
            var parentArgDescription = parentCommand?.ArgumentsRule?.Description;

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

            if (shouldWriteParentCommandArguments)
            {
                WriteColumnizedSummary(
                    $"  <{parentArgName}>",
                    parentArgDescription,
                    15,
                    helpView);
            }

            if (shouldWriteCommandArguments)
            {
                WriteColumnizedSummary(
                    $"  <{name}>",
                    description,
                    15,
                    helpView);
            }
        }



        private static void WriteOptionsSection(
            Command command,
            StringBuilder helpView)
        {
            var options = command
                .DefinedOptions
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
                .DefinedOptions
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
            Option[] options,
            StringBuilder helpView)
        {
            var leftColumnTextFor = options
                .ToDictionary(o => o, LeftColumnText);

            var leftColumnWidth = leftColumnTextFor
                                      .Values
                                      .Select(s => s.Length)
                                      .OrderBy(length => length)
                                      .Last() + 3;

          
            helpView.AppendLine();
            helpView.AppendLine(OptionsSection.Title);

            foreach (var option in options)
            {
                WriteColumnizedSummary(leftColumnTextFor[option],
                                       option.HelpText,
                                       leftColumnWidth,
                                       helpView);
            }
        }

        private static string LeftColumnText(Option option)
        {
            var leftColumnText = "  " +
                                 string.Join(", ",
                                             option.Aliases
                                                   .OrderBy(a => a.Length)
                                                   .Select(a =>
                                                   {
                                                       if (option.IsCommand)
                                                       {
                                                           return a;
                                                       }
                                                       else
                                                       {
                                                           return a.Length == 1
                                                                      ? $"-{a}"
                                                                      : $"--{a}";
                                                       }
                                                   }));

            var argumentName = option.ArgumentsRule.Name;

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
                .RecurseWhileNotNull(c => c.Parent as Command)
                .Reverse())
            {
                helpView.Append($" {subcommand.Name}");

                var argsName = subcommand.ArgumentsRule.Name;
                if (subcommand != command &&
                    !string.IsNullOrWhiteSpace(argsName))
                {
                    helpView.Append($" <{argsName}>");
                }
            }

            if (command.DefinedOptions
                       .Any(o => !o.IsCommand &&
                                 !o.IsHidden()))
            {
                helpView.Append(Synopsis.Options);
            }

            var argumentsName = command.ArgumentsRule.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                helpView.Append($" <{argumentsName}>");
            }

            if (command.DefinedOptions.OfType<Command>().Any())
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