using System;
using System.Linq;
using System.Text;

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

            WriteUsageSummary(command, helpView);

            WriteArgumentsSection(command, helpView);

            WriteOptionsSection(command, helpView);

            WriteSubcommandsSection(command, helpView);

            return helpView.ToString();
        }

        private static void WriteArgumentsSection(
            Command command,
            StringBuilder helpView)
        {
            var name = command.ArgumentsRule.Name;
            var description = command.ArgumentsRule.Description;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(description))
            {
                return;
            }

            helpView.AppendLine();
            helpView.AppendLine("Arguments:");
            helpView.AppendLine($"  <{name}>  {description}");
        }

        private static void WriteSubcommandsSection(
            Command command,
            StringBuilder helpView)
        {
            var subcommands = command
                .DefinedOptions
                .OfType<Command>()
                .ToArray();

            if (!subcommands.Any())
            {
                return;
            }

            helpView.AppendLine();
            helpView.AppendLine("Commands:");

            foreach (var subcommand in subcommands)
            {
                WriteHelpSummary(subcommand, helpView);
            }
        }

        private static void WriteOptionsSection(
            Command command,
            StringBuilder helpView)
        {
            var options = VisibleOptions(command);

            if (!options.Any())
            {
                return;
            }

            helpView.AppendLine();
            helpView.AppendLine("Options:");

            foreach (var option in options)
            {
                WriteHelpSummary(option, helpView);
            }
        }

        public static string HelpView(this Option option)
        {
            var command = option as Command;
            if (command != null)
            {
                return command.HelpView();
            }

            var helpView = new StringBuilder();

            WriteHelpSummary(option, helpView);

            return helpView.ToString();
        }

        private static Option[] VisibleOptions(
            this Command command) =>
            command.DefinedOptions
                   .Where(o => !o.IsCommand)
                   .Where(o => !o.IsHidden())
                   .ToArray();

        private static void WriteHelpSummary(
            Option option,
            StringBuilder helpView)
        {
            var aliases = "    " +
                          string.Join(", ",
                                      option.Aliases
                                            .OrderBy(a => a.Length)
                                            .Select(a =>
                                                        option.IsCommand
                                                            ? a
                                                            : a.Length == 1
                                                                ? $"-{a}"
                                                                : $"--{a}"));

            var argumentName = option.ArgumentsRule.Name;
            if (!string.IsNullOrWhiteSpace(argumentName))
            {
                aliases += $" <{argumentName}>";
            }

            helpView.Append(aliases);

            var colWidth = 38;

            if (aliases.Length <= colWidth - 2)
            {
                helpView.Append(new string(' ', colWidth - aliases.Length));
            }
            else
            {
                helpView.AppendLine();
                helpView.Append(new string(' ', colWidth));
            }

            helpView.AppendLine(
                string.Join(
                    Environment.NewLine + new string(' ', colWidth), option
                        .HelpText
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())));
        }

        private static void WriteUsageSummary(
            Command command,
            StringBuilder helpView)
        {
            helpView.Append("Usage:");

            foreach (var subcommand in command.RecurseWhileNotNull(c => c.Parent as Command)
                                              .Reverse())
            {
                helpView.Append($" {subcommand.Name}");

                var argsName = subcommand.ArgumentsRule.Name;
                if (!string.IsNullOrWhiteSpace(argsName))
                {
                    helpView.Append($" <{argsName}>");
                }
            }

            if (command.DefinedOptions
                       .Any(o => !o.IsCommand &&
                                 !o.IsHidden()))
            {
                helpView.Append($" [options]");
            }

            if (command.DefinedOptions.OfType<Command>().Any())
            {
                helpView.Append(" [command]");
            }

            helpView.AppendLine();
        }
    }
}