﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace System.CommandLine.Help
{
    public class HelpBuilder : IHelpBuilder
    {
        private const string Indent = "  ";

        private Dictionary<ISymbol, Customization> Customizations { get; } = new();

        protected Resources Resources { get; }
        public int MaxWidth { get; }

        public HelpBuilder(Resources resources, int maxWidth = int.MaxValue)
        {
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
            if (maxWidth <= 0) maxWidth = int.MaxValue;
            MaxWidth = maxWidth;
        }

        public virtual void Write(ICommand command, TextWriter writer)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (command.IsHidden)
            {
                return;
            }


            AddSynopsis(command, writer);
            AddUsage(command, writer);
            AddCommandArguments(command, writer);
            AddOptions(command, writer);
            AddSubcommands(command, writer);
            AddAdditionalArguments(command, writer);
        }

        protected internal void Customize(ISymbol symbol,
            Func<string?>? descriptor = null,
            Func<string?>? defaultValue = null)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            Customizations[symbol] = new Customization(descriptor, defaultValue);
        }

        protected virtual void AddSynopsis(ICommand command, TextWriter writer)
        {
            WriteHeading(Resources.Instance.HelpDescriptionTitle(), command.Description, writer);
            writer.WriteLine();
        }

        protected virtual void AddUsage(ICommand command, TextWriter writer)
        {
            string description = GetUsage(command);
            WriteHeading(Resources.HelpUsageTitle(), description, writer);
            writer.WriteLine();
        }

        protected string GetUsage(ICommand command)
        {
            return string.Join(" ", GetUsageParts().Where(x => !string.IsNullOrWhiteSpace(x)));

            IEnumerable<string> GetUsageParts()
            {

                IEnumerable<ICommand> parentCommands =
                    command
                        .RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                        .Reverse();

                var displayOptionTitle = command.Options.Any(x => !x.IsHidden);

                foreach (ICommand parentCommand in parentCommands)
                {
                    yield return parentCommand.Name;

                    if (displayOptionTitle)
                    {
                        yield return Resources.HelpUsageOptionsTitle();
                        displayOptionTitle = false;
                    }

                    yield return FormatArgumentUsage(parentCommand.Arguments);
                }


                var hasCommandWithHelp = command.Children
                    .OfType<ICommand>()
                    .Any(x => !x.IsHidden);

                if (hasCommandWithHelp)
                {
                    yield return Resources.HelpUsageCommandTitle();
                }

                if (!command.TreatUnmatchedTokensAsErrors)
                {
                    yield return Resources.HelpUsageAdditionalArguments();
                }
            }
        }

        protected virtual void AddCommandArguments(ICommand command, TextWriter writer)
        {
            HelpItem[] commandArguments = GetCommandArguments(command).ToArray();

            if (commandArguments.Length > 0)
            {
                WriteHeading(Resources.HelpArgumentsTitle(), null, writer);
                RenderAsColumns(writer, commandArguments);
                writer.WriteLine();
            }
        }

        protected IEnumerable<HelpItem> GetCommandArguments(ICommand command)
        {
            //TODO: This shows all parent arguments not just the first level
            return command.RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                    .Reverse()
                    .SelectMany(GetArguments)
                    .Distinct();

            IEnumerable<HelpItem> GetArguments(ICommand command)
            {
                var arguments = command.Arguments.Where(x => !x.IsHidden).ToList();

                foreach (IArgument argument in arguments)
                {
                    string argumentDescriptor = GetArgumentDescriptor(argument);

                    yield return new HelpItem(argumentDescriptor, string.Join(" ", GetArgumentDescription(command, argument)));
                }
            }

            IEnumerable<string> GetArgumentDescription(IIdentifierSymbol parent, IArgument argument)
            {
                string? description = argument.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }

                if (argument.HasDefaultValue)
                {
                    yield return $"[{GetArgumentDefaultValue(parent, argument, true)}]";
                }
            }
        }

        protected virtual void AddOptions(ICommand command, TextWriter writer)
        {
            var options = GetOptions(command).ToArray();

            if (options.Length > 0)
            {
                WriteHeading(Resources.HelpOptionsTitle(), null, writer);
                RenderAsColumns(writer, options);
                writer.WriteLine();
            }
        }

        protected IEnumerable<HelpItem> GetOptions(ICommand command)
            => command.Options.Where(x => !x.IsHidden).Select(GetHelpItem);

        protected virtual void AddSubcommands(ICommand command, TextWriter writer)
        {
            var subcommands = GetSubcommands(command).ToArray();

            if (subcommands.Length > 0)
            {
                WriteHeading(Resources.HelpCommandsTitle(), null, writer);
                RenderAsColumns(writer, subcommands);
                writer.WriteLine();
            }
        }

        protected IEnumerable<HelpItem> GetSubcommands(ICommand command)
            => command.Children.OfType<ICommand>().Where(x => !x.IsHidden).Select(GetHelpItem);

        protected virtual void AddAdditionalArguments(ICommand command, TextWriter writer)
        {
            if (command.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            WriteHeading(Resources.HelpAdditionalArgumentsTitle(),
                Resources.HelpAdditionalArgumentsDescription(), writer);
        }

        protected void WriteHeading(string descriptor, string? description, TextWriter writer)
        {
            if (!string.IsNullOrWhiteSpace(descriptor))
            {
                writer.WriteLine(descriptor);
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                int maxWidth = MaxWidth - Indent.Length;
                foreach (var part in WrapItem(description!, maxWidth))
                {
                    writer.Write(Indent);
                    writer.WriteLine(part);
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

        protected void RenderAsColumns(TextWriter writer, params HelpItem[] items)
        {
            if (items.Length == 0)
            {
                return;
            }

            int windowWidth = MaxWidth;

            int firstColumnWidth = items.Select(x => x.Descriptor.Length).Max();
            int secondColumnWidth = items.Select(x => x.Description.Length).Max();

            if (firstColumnWidth + secondColumnWidth + Indent.Length + Indent.Length > windowWidth)
            {
                int firstColumnMaxWidth = windowWidth / 2 - Indent.Length;
                if (firstColumnWidth > firstColumnMaxWidth)
                {
                    firstColumnWidth = items.SelectMany(x => WrapItem(x.Descriptor, firstColumnMaxWidth).Select(x => x.Length)).Max();
                }
                secondColumnWidth = windowWidth - firstColumnWidth - Indent.Length - Indent.Length;
            }

            foreach (var (descriptor, description) in items)
            {
                IEnumerable<string> descriptorParts = WrapItem(descriptor, firstColumnWidth);
                IEnumerable<string> descriptionParts = WrapItem(description, secondColumnWidth);

                foreach (var (first, second) in ZipWithEmpty(descriptorParts, descriptionParts))
                {
                    writer.Write($"{Indent}{first}");
                    if (!string.IsNullOrWhiteSpace(second))
                    {
                        int padSize = firstColumnWidth - first.Length;
                        string padding = "";
                        if (padSize > 0)
                        {
                            padding = new string(' ', padSize);
                        }
                        writer.Write($"{padding}{Indent}{second}");
                    }
                    writer.WriteLine();
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
            if (string.IsNullOrWhiteSpace(item))
            {
                yield break;
            }

            //First handle existing new lines
            var parts = item.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

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

        protected HelpItem GetHelpItem(IIdentifierSymbol symbol)
        {
            string descriptor;
            if (Customizations.TryGetValue(symbol, out Customization customization) &&
                customization.GetDescriptor?.Invoke() is { } setDescriptor)
            {
                descriptor = setDescriptor;
            }
            else
            {
                var rawAliases = symbol.Aliases
                                 .Select(r => r.SplitPrefix())
                                 .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                                 .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                                 .GroupBy(t => t.Alias)
                                 .Select(t => t.First())
                                 .Select(t => $"{t.Prefix}{t.Alias}");

                descriptor = string.Join(", ", rawAliases);

                foreach (var argument in symbol.Arguments())
                {
                    if (!argument.IsHidden)
                    {
                        var argumentDescriptor = GetArgumentDescriptor(argument);
                        if (!string.IsNullOrWhiteSpace(argumentDescriptor))
                        {
                            descriptor += $" {argumentDescriptor}";
                        }
                    }
                }

                if (symbol is IOption option &&
                    option.IsRequired)
                {
                    descriptor += $" {Resources.HelpOptionsRequired()}";
                }
            }

            return new HelpItem(descriptor, GetDescription(symbol));
        }

        protected string GetDescription(IIdentifierSymbol symbol)
        {
            return string.Join(" ", GetDescriptionParts(symbol));

            IEnumerable<string> GetDescriptionParts(IIdentifierSymbol symbol)
            {
                string? description = symbol.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }
                string argumentsDescription = GetArgumentsDescription(symbol);
                if (!string.IsNullOrWhiteSpace(argumentsDescription))
                {
                    yield return argumentsDescription;
                }
            }

            string GetArgumentsDescription(IIdentifierSymbol symbol)
            {
                IEnumerable<IArgument> arguments = symbol.Arguments();
                var defaultArguments = arguments.Where(x => !x.IsHidden && x.HasDefaultValue).ToArray();

                if (defaultArguments.Length == 0) return "";

                var isSingleArgument = defaultArguments.Length == 1;
                var argumentDefaultValues = defaultArguments
                    .Select(argument => GetArgumentDefaultValue(symbol, argument, isSingleArgument));
                return $"[{string.Join(", ", argumentDefaultValues)}]";
            }
        }

        private string GetArgumentDefaultValue(IIdentifierSymbol parent, IArgument argument, bool displayArgumentName)
        {
            string? defaultValue;
            if (Customizations.TryGetValue(parent, out Customization customization) &&
                customization.GetDefaultValue?.Invoke() is { } parentSetDefaultValue)
            {
                defaultValue = parentSetDefaultValue;
            }
            else if (Customizations.TryGetValue(argument, out customization) &&
                customization.GetDefaultValue?.Invoke() is { } setDefaultValue)
            {
                defaultValue = setDefaultValue;
            }
            else
            {
                object? argumentDefaultValue = argument.GetDefaultValue();
                if (argumentDefaultValue is IEnumerable enumerable && !(argumentDefaultValue is string))
                {
                    defaultValue = string.Join("|", enumerable.OfType<object>().ToArray());
                }
                else
                {
                    defaultValue = argumentDefaultValue?.ToString();
                }
            }

            string name = displayArgumentName ?
                Resources.HelpArgumentDefaultValueTitle() :
                argument.Name;

            return $"{name}: {defaultValue}";
        }

        protected string GetArgumentDescriptor(IArgument argument)
        {
            if (Customizations.TryGetValue(argument, out Customization customization) &&
                customization.GetDescriptor?.Invoke() is { } setDescriptor)
            {
                return setDescriptor;
            }

            if (argument.ValueType == typeof(bool) ||
                argument.ValueType == typeof(bool?))
            {
                if (argument.Parents.FirstOrDefault() is ICommand)
                {
                    return $"<{argument.Name}>";
                }
                else
                {
                    return "";
                }
            }

            string descriptor;
            var suggestions = argument.GetSuggestions().ToArray();
            var helpName = GetArgumentHelpName(argument);
            if (!string.IsNullOrEmpty(helpName))
            {
                descriptor = helpName!;
            }
            else if (suggestions.Length > 0)
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

        private string? GetArgumentHelpName(IArgument argument)
        {
            var arg = argument as Argument;
            return arg?.HelpName;
        }

        private class Customization
        {
            public Customization(Func<string?>? getDescriptor,
                Func<string?>? getDefaultValue)
            {
                GetDescriptor = getDescriptor;
                GetDefaultValue = getDefaultValue;
            }

            public Func<string?>? GetDescriptor { get; }
            public Func<string?>? GetDefaultValue { get; }
        }
    }
}
