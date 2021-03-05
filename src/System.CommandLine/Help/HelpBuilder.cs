// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Help
{
    public class HelpBuilder : IHelpBuilder
    {
        private const string Indent = "  ";

        protected IConsole Console { get; }

        public HelpBuilder(IConsole console)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }


        public virtual void Write(ICommand command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (command.IsHidden)
            {
                return;
            }

            AddSynopsis(command);
            AddUsage(command);
            AddArguments(command);
            AddOptions(command);
            AddSubcommands(command);
            AddAdditionalArguments(command);
        }

        protected virtual void AddSynopsis(ICommand command)
        {
            WriteHeading(command.Name, command.Description);
            Console.Out.WriteLine();
        }

        protected virtual void AddUsage(ICommand command)
        {
            string description = string.Join(" ", GetUsageParts().Where(x => !string.IsNullOrWhiteSpace(x)));
            WriteHeading(Resources.Instance.HelpUsageTile(), description);

            IEnumerable<string> GetUsageParts()
            {
                IEnumerable<ICommand> parentCommands =
                    command
                        .RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                        .Reverse();

                foreach (var subcommand in parentCommands)
                {
                    yield return subcommand.Name;

                    //TODO???
                    //if (subcommand != command)
                    //{
                    //    usage.Add(FormatArgumentUsage(subcommand.Arguments));
                    //}
                }

                var hasOptionWithHelp = command.Options.Any(x => !x.IsHidden);

                if (hasOptionWithHelp)
                {
                    yield return Resources.Instance.HelpUsageOptionsTile();
                }

                //TODO???
                //usage.Add(FormatArgumentUsage(command.Arguments));

                var hasCommandWithHelp = command.Children
                    .OfType<ICommand>()
                    .Any(x => !x.IsHidden);

                if (hasCommandWithHelp)
                {
                    yield return Resources.Instance.HelpUsageCommandTile();
                }

                if (!command.TreatUnmatchedTokensAsErrors)
                {
                    yield return Resources.Instance.HelpUsageAdditionalArguments();
                }
            }
        }

        protected virtual void AddArguments(ICommand command)
        {
            WriteHeading(Resources.Instance.HelpArgumentsTitle(), null);
            //TODO: This shows all parent arguments not just the first level
            (string, string)[]? commandArguments =
                    command.RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                    .SelectMany(GetArguments)
                    .ToArray();

            RenderAsColumns(commandArguments);

            static IEnumerable<(string, string)> GetArguments(ICommand command)
            {
                var arguments = command.Arguments.Where(x => !x.IsHidden).ToList();
                foreach (IArgument argument in arguments)
                {
                    string argumentDescriptor = ArgumentDescriptor(argument);

                    yield return (argumentDescriptor, string.Join(" ", GetArgumentDescription(argument, arguments.Count == 1)));
                }
            }
        }

        protected virtual void AddOptions(ICommand command)
        {
            WriteHeading(Resources.Instance.HelpOptionsTitle(), null);
            
            var options = command
                          .Options
                          .Where(x => !x.IsHidden)
                          .Select(GetSymbolParts)
                          .ToArray();

            RenderAsColumns(options);
        }

        protected virtual void AddSubcommands(ICommand command)
        {
            WriteHeading(Resources.Instance.HelpCommandsTitle(), null);

            var subcommands = command
                              .Children
                              .OfType<ICommand>()
                              .Where(x => !x.IsHidden)
                              .Select(GetSymbolParts)
                              .ToArray();
            
            RenderAsColumns(subcommands);
        }

        protected virtual void AddAdditionalArguments(ICommand command)
        {
            if (command.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            WriteHeading(Resources.Instance.HelpAdditionalArgumentsTitle(),
                Resources.Instance.HelpAdditionalArgumentsDescription());
        }

        protected void WriteHeading(string name, string? description)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Console.Out.WriteLine(name);
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                Console.Out.Write(Indent);
                Console.Out.WriteLine(description!);
            }
        }

        private void RenderAsColumns(params (string First, string Second)[] items)
        {
            if (items.Length == 0) return;
            int windowWidth = GetConsoleWindowWidth(Console);
            int firstColumnWidth = items.Select(x => x.First.Length).Max();
            foreach (var (name, value) in items)
            {
                int padSize = firstColumnWidth - name.Length;
                string padding = "";
                if (padSize > 0)
                {
                    padding = new string(' ', padSize);
                }
                Console.Out.WriteLine($"{Indent}{name}{padding}{Indent}{value}");
            }
        }

        private static (string, string) GetSymbolParts(IIdentifierSymbol symbol)
        {
            var rawAliases = symbol
                             .Aliases
                             .Select(r => r.SplitPrefix())
                             .OrderBy(r => r.prefix, StringComparer.OrdinalIgnoreCase)
                             .ThenBy(r => r.alias, StringComparer.OrdinalIgnoreCase)
                             .GroupBy(t => t.alias)
                             .Select(t => t.First())
                             .Select(t => $"{t.prefix}{t.alias}");

            var invocation = string.Join(", ", rawAliases);

            foreach (var argument in symbol.Arguments())
            {
                if (!argument.IsHidden)
                {
                    var argumentDescriptor = ArgumentDescriptor(argument);
                    if (!string.IsNullOrWhiteSpace(argumentDescriptor))
                    {
                        invocation += $" {argumentDescriptor}";
                    }
                }
            }

            if (symbol is IOption option &&
                option.IsRequired)
            {
                invocation += $" {Resources.Instance.HelpOptionsRequired()}";
            }

            return (invocation, string.Join(" ", GetDescriptionParts(symbol)));

            static IEnumerable<string> GetDescriptionParts(IIdentifierSymbol symbol)
            {
                string? description = symbol.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }
                string argumentsDescription = GetArgumentsDescription(symbol.Arguments());
                if (!string.IsNullOrWhiteSpace(argumentsDescription))
                {
                    yield return argumentsDescription;
                }
            }
        }

        private static string GetArgumentsDescription(IEnumerable<IArgument> arguments)
        {
            var defaultArguments = arguments.Where(x => !x.IsHidden && x.HasDefaultValue).ToArray();

            if (defaultArguments.Length == 0) return "";

            var isSingleArgument = defaultArguments.Length == 1;
            var argumentDefaultValues = defaultArguments
                .Select(argument => GetArgumentDefaultValue(argument, isSingleArgument));
            return $"[{string.Join(", ", argumentDefaultValues)}]";
        }

        private static IEnumerable<string> GetArgumentDescription(IArgument argument, bool isSingleArgument)
        {
            string? description = argument.Description;
            if (!string.IsNullOrWhiteSpace(description))
            {
                yield return description!;
            }

            if (argument.HasDefaultValue)
            {
                yield return GetArgumentDefaultValue(argument, isSingleArgument);
            }
        }

        private static string GetArgumentDefaultValue(IArgument argument, bool isSingleArgument)
        {
            string name = isSingleArgument ?
                Resources.Instance.HelpArgumentDefaultValueTitle() :
                argument.Name;
            
            return $"{name}: {argument.GetDefaultValue()}";
        }

        protected static string ArgumentDescriptor(IArgument argument)
        {
            if (argument.ValueType == typeof(bool) ||
                argument.ValueType == typeof(bool?))
            {
                return "";
            }

            var suggestions = argument.GetSuggestions().ToArray();
            string descriptor;
            if (suggestions.Length > 0)
            {
                descriptor = string.Join("|", suggestions);
            }
            else
            {
                descriptor = argument.Name;
            }

            if (!string.IsNullOrWhiteSpace(descriptor))
            {
                return $"<{descriptor}>";
            }
            return descriptor;
        }
        private int GetConsoleWindowWidth(IConsole console)
        {
            if (console is SystemConsole systemConsole)
            {
                return systemConsole.GetConsoleWindowWidth();
            }
            else
            {
                return int.MaxValue;
            }
        }
    }
}
