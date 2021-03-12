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
            Console.Out.WriteLine();

            IEnumerable<string> GetUsageParts()
            {
                IEnumerable<ICommand> parentCommands =
                    command
                        .RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                        .Reverse();

                foreach (var subcommand in parentCommands)
                {
                    yield return subcommand.Name;

                    yield return FormatArgumentUsage(subcommand.Arguments);
                }

                var hasOptionWithHelp = command.Options.Any(x => !x.IsHidden);

                if (hasOptionWithHelp)
                {
                    yield return Resources.Instance.HelpUsageOptionsTile();
                }

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
            //TODO: This shows all parent arguments not just the first level
            (string, string)[]? commandArguments =
                    command.RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                    .Reverse()
                    .SelectMany(GetArguments)
                    .Distinct()
                    .ToArray();

            if (commandArguments.Length > 0)
            {
                WriteHeading(Resources.Instance.HelpArgumentsTitle(), null);
                RenderAsColumns(commandArguments);
                Console.Out.WriteLine();
            }

            static IEnumerable<(string, string)> GetArguments(ICommand command)
            {
                var arguments = command.Arguments.Where(x => !x.IsHidden).ToList();
                foreach (IArgument argument in arguments)
                {
                    string argumentDescriptor = ArgumentDescriptor(argument);

                    yield return (argumentDescriptor, string.Join(" ", GetArgumentDescription(argument, true)));
                }
            }
        }

        protected virtual void AddOptions(ICommand command)
        {
            var options = command
                          .Options
                          .Where(x => !x.IsHidden)
                          .Select(GetSymbolParts)
                          .ToArray();

            if (options.Length > 0)
            {
                WriteHeading(Resources.Instance.HelpOptionsTitle(), null);
                RenderAsColumns(options);
                Console.Out.WriteLine();
            }
        }

        protected virtual void AddSubcommands(ICommand command)
        {
            var subcommands = command
                              .Children
                              .OfType<ICommand>()
                              .Where(x => !x.IsHidden)
                              .Select(GetSymbolParts)
                              .ToArray();

            if (subcommands.Length > 0)
            {
                WriteHeading(Resources.Instance.HelpCommandsTitle(), null);
                RenderAsColumns(subcommands);
                Console.Out.WriteLine();
            }
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
            WriteIndented(description);
        }

        protected void WriteIndented(string? text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                int maxWidth = GetConsoleWindowWidth(Console) - Indent.Length;
                foreach (var part in WrapItem(text!, maxWidth))
                {
                    Console.Out.Write(Indent);
                    Console.Out.WriteLine(part);
                }
            }
        }

        private string FormatArgumentUsage(IReadOnlyList<IArgument> arguments)
        {
            var sb = StringBuilderPool.Default.Rent();

            try
            {
                var end = default(Stack<char>);

                for (var i = 0; i < arguments.Count; i++)
                {
                    var argument = arguments[i];
                    if (argument.IsHidden)
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
                        (end ??= new Stack<char>()).Push(']');
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
                            sb.Append(end.Pop());
                        }
                    }
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Default.ReturnToPool(sb);
            }

            bool IsMultiParented(IArgument argument) =>
                argument is Argument a &&
                a.Parents.Count > 1;

            bool IsOptional(IArgument argument) =>
                IsMultiParented(argument) ||
                argument.Arity.MinimumNumberOfValues == 0;
        }

        private void RenderAsColumns(params (string First, string Second)[] items)
        {
            //TODO: allow for more customization of this layout...
            if (items.Length == 0) return;
            int windowWidth = GetConsoleWindowWidth(Console);
            //int firstColumnWidth = Math.Min(items.Select(x => x.First.Length).Max(), windowWidth / 2 - Indent.Length);
            int firstColumnWidth = items.Select(x => x.First.Length).Max();
            int firstColumnMaxWidth = windowWidth / 2 - Indent.Length;
            if (firstColumnWidth > firstColumnMaxWidth)
            {
                firstColumnWidth = items.SelectMany(x => WrapItem(x.First, firstColumnMaxWidth).Select(x => x.Length)).Max();
            }
            int secondColumnWidth = windowWidth - firstColumnWidth - Indent.Length - Indent.Length;

            foreach (var (name, value) in items)
            {
                IEnumerable<string> nameParts = WrapItem(name, firstColumnWidth);
                IEnumerable<string> valueParts = WrapItem(value, secondColumnWidth);

                foreach (var (first, second) in ZipWithEmpty(nameParts, valueParts))
                {
                    Console.Out.Write($"{Indent}{first}");
                    if (!string.IsNullOrWhiteSpace(second))
                    {
                        int padSize = firstColumnWidth - first.Length;
                        string padding = "";
                        if (padSize > 0)
                        {
                            padding = new string(' ', padSize);
                        }
                        Console.Out.Write($"{padding}{Indent}{second}");
                    }
                    Console.Out.WriteLine();
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

        private static IEnumerable<string> WrapItem(string item, int maxWidth)
        {
            if (string.IsNullOrWhiteSpace(item)) yield break;
            //First handle existing new lines
            var parts = item.Split(new string[] { "\r\n", "\n", }, StringSplitOptions.None);

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

        //TODO: isSingleArgument is a bad name. Appears to be more about default display
        private static IEnumerable<string> GetArgumentDescription(IArgument argument, bool isSingleArgument)
        {
            string? description = argument.Description;
            if (!string.IsNullOrWhiteSpace(description))
            {
                yield return description!;
            }

            if (argument.HasDefaultValue)
            {
                yield return $"[{GetArgumentDefaultValue(argument, isSingleArgument)}]";
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
            if (console is IConsoleWindow consoleWindow)
            {
                return consoleWindow.GetWindowWidth();
            }
            else
            {
                return int.MaxValue;
            }
        }
    }
}
