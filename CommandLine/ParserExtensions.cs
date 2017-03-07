// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ParserExtensions
    {
        public static string HelpView(this Parser parser) =>
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
            var options = command.DefinedOptions.ToArray();

            options = options
                .Where(o => !o.IsCommand)
                .Where(o => !o.IsHidden())
                .ToArray();

            var s = new StringBuilder();

            s.Append("usage: ");

            s.Append(command.FullyQualifiedName());

            if (options.Any())
            {
                s.Append(" [options]");
            }

            var argumentName = command.ArgumentsRule.Name;
            if (!string.IsNullOrWhiteSpace(argumentName))
            {
                s.Append($" [{argumentName}]");
            }

            s.AppendLine();

            if (options.Any())
            {
                s.AppendLine();
                s.AppendLine("Options:");
                s.AppendLine();

                foreach (var option in options)
                {
                    WriteHelpSummary(option, s);
                }
            }

            var subcommands = command
                .DefinedOptions
                .OfType<Command>()
                .ToArray();

            if (subcommands.Any())
            {
                s.AppendLine();
                s.AppendLine("Commands:");
                foreach (var subcommand in subcommands)
                {
                    WriteHelpSummary(subcommand, s);
                }
            }

            return s.ToString();
        }

        public static string HelpView(this Option option)
        {
            var command = option as Command;
            if (command != null)
            {
                return command.HelpView();
            }

            var s = new StringBuilder();

            WriteHelpSummary(option, s);

            return s.ToString();
        }

        private static void WriteHelpSummary(Option option, StringBuilder s)
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

            s.Append(aliases);

            var colWidth = 38;

            if (aliases.Length <= colWidth - 2)
            {
                s.Append(new string(' ', colWidth - aliases.Length));
            }
            else
            {
                s.AppendLine();
                s.Append(new string(' ', colWidth));
            }

            s.AppendLine(option.HelpText
                               .Replace(Environment.NewLine,
                                        Environment.NewLine + new string(' ', colWidth)));
        }

        public static ParseResult Parse(this Parser parser, string s) =>
            parser.Parse(s.Tokenize().ToArray(),
                         isProgressive: !s.EndsWith(" "));
    }
}