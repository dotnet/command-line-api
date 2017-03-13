using System;
using System.Collections.Generic;
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
            }

            var options = command.DefinedOptions.ToArray();

            options = options
                .Where(o => !o.IsCommand)
                .Where(o => !o.IsHidden())
                .ToArray();

            WriteUsageSummary(command, options, helpView);

            WriteOptionsSection(options, helpView);

            WriteArgumentsSection(command, helpView);

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
            IReadOnlyCollection<Option> options,
            StringBuilder helpView)
        {
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

        private static void WriteUsageSummary(
            Command command,
            IReadOnlyCollection<Option> options,
            StringBuilder helpView)
        {
            helpView.Append("Usage:");

            foreach (var c in command.RecurseWhileNotNull(c => c.Parent as Command)
                                     .Reverse())
            {
                helpView.Append($" {c.Name}");
                var argsName = c.ArgumentsRule.Name;
                if (!string.IsNullOrWhiteSpace(argsName))
                {
                    helpView.Append($" [{argsName}]");
                }
            }

            if (options.Any())
            {
                helpView.Append(" [options]");
            }

            helpView.AppendLine();
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
    }
}