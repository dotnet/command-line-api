﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace System.CommandLine.Help
{
    /// <inheritdoc />
    public class HelpBuilder : IHelpBuilder
    {
        private const string Indent = "  ";

        private Dictionary<ISymbol, Customization> Customizations { get; } = new();

        /// <param name="localizationResources">Resources used to localize the help output.</param>
        /// <param name="maxWidth">The maximum width in characters after which help output is wrapped.</param>
        public HelpBuilder(LocalizationResources localizationResources, int maxWidth = int.MaxValue)
        {
            LocalizationResources = localizationResources ?? throw new ArgumentNullException(nameof(localizationResources));
            if (maxWidth <= 0) maxWidth = int.MaxValue;
            MaxWidth = maxWidth;
        }

        /// <summary>
        /// Provides localizable strings for help and error messages.
        /// </summary>
        protected LocalizationResources LocalizationResources { get; }

        /// <summary>
        /// The maximum width for which to format help output.
        /// </summary>
        public int MaxWidth { get; }

        /// <summary>
        /// Writes help for the specified command.
        /// </summary>
        /// <param name="command">The command to write help output for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        public virtual void Write(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            if (parseResult is null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            if (command.IsHidden)
            {
                return;
            }

            AddSynopsis(command, writer);
            AddUsage(command, writer);
            AddCommandArguments(command, writer, parseResult);
            AddOptions(command, writer, parseResult);
            AddSubcommands(command, writer, parseResult);
            AddAdditionalArguments(command, writer);
        }

        /// <summary>
        /// Specifies custom help details for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to specify custom help details for.</param>
        /// <param name="descriptor">A delegate to display the name and invocation details, typically in the first help column.</param>
        /// <param name="description">A delegate to display the description of the symbol, typically in the second help column.</param>
        /// <param name="defaultValue">A delegate to display the default value for the symbol.</param>
        protected internal void Customize(ISymbol symbol,
            Func<ParseResult?, string?>? descriptor = null,
            Func<ParseResult?, string?>? description = null,
            Func<ParseResult?, string?>? defaultValue = null)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            Customizations[symbol] = new Customization(descriptor, description, defaultValue);
        }

        /// <summary>
        /// Writes the synopsis for the specified command.
        /// </summary>
        /// <param name="command">The command to write help details for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        protected virtual void AddSynopsis(ICommand command, TextWriter writer)
        {
            WriteHeading(LocalizationResources.Instance.HelpDescriptionTitle(), command.Description, writer);
            writer.WriteLine();
        }

        /// <summary>
        /// Writes usage for the specified command.
        /// </summary>
        /// <param name="command">The command to write help details for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        protected virtual void AddUsage(ICommand command, TextWriter writer)
        {
            string description = GetUsage(command);
            WriteHeading(LocalizationResources.HelpUsageTitle(), description, writer);
            writer.WriteLine();
        }

        /// <summary>
        /// Gets the usage for the specified command.
        /// </summary>
        /// <param name="command">The command to get usage for.</param>
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
                        yield return LocalizationResources.HelpUsageOptionsTitle();
                        displayOptionTitle = false;
                    }

                    yield return FormatArgumentUsage(parentCommand.Arguments);
                }

                var hasCommandWithHelp = command.Children
                    .OfType<ICommand>()
                    .Any(x => !x.IsHidden);

                if (hasCommandWithHelp)
                {
                    yield return LocalizationResources.HelpUsageCommandTitle();
                }

                if (!command.TreatUnmatchedTokensAsErrors)
                {
                    yield return LocalizationResources.HelpUsageAdditionalArguments();
                }
            }
        }

        /// <summary>
        /// Writes help output for the specified command's arguments.
        /// </summary>
        /// <param name="command">The command to write out argument help for.</param>
        /// <param name="writer">The writer to write help output to.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        protected virtual void AddCommandArguments(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            HelpItem[] commandArguments = GetCommandArguments(command, parseResult).ToArray();

            if (commandArguments.Length > 0)
            {
                WriteHeading(LocalizationResources.HelpArgumentsTitle(), null, writer);
                RenderAsColumns(writer, commandArguments);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets help items for the specified command's arguments.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        /// <returns>Help items for the specified command's arguments.</returns>
        protected IEnumerable<HelpItem> GetCommandArguments(ICommand command, ParseResult parseResult)
        {
            //TODO: This shows all parent arguments not just the first level
            return command.RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
                    .Reverse()
                    .SelectMany(x => GetArguments(x, parseResult))
                    .Distinct();

            IEnumerable<HelpItem> GetArguments(ICommand command, ParseResult parseResult)
            {
                var arguments = command.Arguments.Where(x => !x.IsHidden).ToList();

                foreach (IArgument argument in arguments)
                {
                    string argumentDescriptor = GetArgumentDescriptor(argument, parseResult);

                    yield return new HelpItem(argumentDescriptor, string.Join(" ", GetArgumentDescription(command, argument, parseResult)));
                }
            }

            IEnumerable<string> GetArgumentDescription(IIdentifierSymbol parent, IArgument argument, ParseResult parseResult)
            {
                string? description = argument.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }

                if (argument.HasDefaultValue)
                {
                    yield return $"[{GetArgumentDefaultValue(parent, argument, true, parseResult)}]";
                }
            }
        }

        /// <summary>
        /// Writes help output for the specified command's options.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        /// <param name="writer">The writer to write help output to.</param>
        protected virtual void AddOptions(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            var options = GetOptions(command, parseResult).ToArray();

            if (options.Length > 0)
            {
                WriteHeading(LocalizationResources.HelpOptionsTitle(), null, writer);
                RenderAsColumns(writer, options);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets help items for the specified command's options.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        protected IEnumerable<HelpItem> GetOptions(ICommand command, ParseResult parseResult)
            => command.Options.Where(x => !x.IsHidden).Select(x => GetHelpItem(x, parseResult));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        /// <param name="writer">The writer to write help output to.</param>
        protected virtual void AddSubcommands(ICommand command, TextWriter writer, ParseResult parseResult)
        {
            var subcommands = GetSubcommands(command, parseResult).ToArray();

            if (subcommands.Length > 0)
            {
                WriteHeading(LocalizationResources.HelpCommandsTitle(), null, writer);
                RenderAsColumns(writer, subcommands);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets help items for the specified command's subcommands.
        /// </summary>
        /// <param name="command">The command to get argument help items for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        protected IEnumerable<HelpItem> GetSubcommands(ICommand command, ParseResult parseResult)
            => command.Children.OfType<ICommand>().Where(x => !x.IsHidden).Select(x => GetHelpItem(x, parseResult));

        /// <summary>
        /// Writes help output for additional arguments.
        /// </summary>
        protected virtual void AddAdditionalArguments(ICommand command, TextWriter writer)
        {
            if (command.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            WriteHeading(LocalizationResources.HelpAdditionalArgumentsTitle(),
                LocalizationResources.HelpAdditionalArgumentsDescription(), writer);
        }

        /// <summary>
        /// Writes a heading to help output.
        /// </summary>
        /// <param name="descriptor">The name and invocation details, typically in the first help column.</param>
        /// <param name="description">The description of the symbol, typically in the second help column.</param>
        /// <param name="writer">The writer to write help output to.</param>
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

        /// <summary>
        /// Writes the specified help items, aligning output in columns.
        /// </summary>
        /// <param name="writer">The writer to write help output to.</param>
        /// <param name="items">The help items to write out in columns.</param>
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

            foreach (var helpItem in items)
            {
                IEnumerable<string> descriptorParts = WrapItem(helpItem.Descriptor, firstColumnWidth);
                IEnumerable<string> descriptionParts = WrapItem(helpItem.Description, secondColumnWidth);

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

        /// <summary>
        /// Gets a help item for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        protected HelpItem GetHelpItem(IIdentifierSymbol symbol, ParseResult parseResult)
        {
            string descriptor;
            if (Customizations.TryGetValue(symbol, out Customization customization) &&
                customization.GetDescriptor?.Invoke(parseResult) is { } setDescriptor)
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
                        var argumentDescriptor = GetArgumentDescriptor(argument, parseResult);
                        if (!string.IsNullOrWhiteSpace(argumentDescriptor))
                        {
                            descriptor += $" {argumentDescriptor}";
                        }
                    }
                }

                if (symbol is IOption option &&
                    option.IsRequired)
                {
                    descriptor += $" {LocalizationResources.HelpOptionsRequired()}";
                }
            }

            return new HelpItem(descriptor, GetDescription(symbol, parseResult));
        }

        /// <summary>
        /// Gets the description for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to get the description for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        protected string GetDescription(IIdentifierSymbol symbol, ParseResult parseResult)
        {
            return string.Join(" ", GetDescriptionParts());

            IEnumerable<string> GetDescriptionParts()
            {
                string? description = symbol.Description;
                if (!string.IsNullOrWhiteSpace(description))
                {
                    yield return description!;
                }
                else if (Customizations.TryGetValue(symbol, out var customization) &&
                    customization.GetDescription?.Invoke(parseResult) is { } descriptionValue)
                {
                    yield return descriptionValue;
                }
                string argumentsDescription = GetArgumentsDescription();
                if (!string.IsNullOrWhiteSpace(argumentsDescription))
                {
                    yield return argumentsDescription;
                }
            }

            string GetArgumentsDescription()
            {
                IEnumerable<IArgument> arguments = symbol.Arguments();
                var defaultArguments = arguments.Where(x => !x.IsHidden && x.HasDefaultValue).ToArray();

                if (defaultArguments.Length == 0) return "";

                var isSingleArgument = defaultArguments.Length == 1;
                var argumentDefaultValues = defaultArguments
                    .Select(argument => GetArgumentDefaultValue(symbol, argument, isSingleArgument, parseResult));
                return $"[{string.Join(", ", argumentDefaultValues)}]";
            }
        }

        private string GetArgumentDefaultValue(IIdentifierSymbol parent, IArgument argument, bool displayArgumentName, ParseResult parseResult)
        {
            string? defaultValue;
            if (Customizations.TryGetValue(parent, out Customization customization) &&
                customization.GetDefaultValue?.Invoke(parseResult) is { } parentSetDefaultValue)
            {
                defaultValue = parentSetDefaultValue;
            }
            else if (Customizations.TryGetValue(argument, out customization) &&
                customization.GetDefaultValue?.Invoke(parseResult) is { } setDefaultValue)
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
                LocalizationResources.HelpArgumentDefaultValueTitle() :
                argument.Name;

            return $"{name}: {defaultValue}";
        }

        /// <summary>
        /// Gets the descriptor for the specified argument.
        /// </summary>
        /// <param name="argument">The argument to get the descriptor for.</param>
        /// <param name="parseResult">A parse result providing context for help formatting.</param>
        protected string GetArgumentDescriptor(IArgument argument, ParseResult parseResult)
        {
            if (Customizations.TryGetValue(argument, out Customization customization) &&
                customization.GetDescriptor?.Invoke(parseResult) is { } setDescriptor)
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
            public Customization(Func<ParseResult?, string?>? getDescriptor,
                Func<ParseResult?, string?>? getDescription,
                Func<ParseResult?, string?>? getDefaultValue)
            {
                GetDescriptor = getDescriptor;
                GetDescription = getDescription;
                GetDefaultValue = getDefaultValue;
            }

            public Func<ParseResult?, string?>? GetDescriptor { get; }
            public Func<ParseResult?, string?>? GetDescription { get; }
            public Func<ParseResult?, string?>? GetDefaultValue { get; }
        }
    }
}
