// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.CommandLine.Help.DefaultHelpText;

namespace System.CommandLine.Help
{
    public class HelpBuilder : IHelpBuilder
    {
        protected const int DefaultColumnGutter = 4;
        protected const int DefaultIndentationSize = 2;
        protected const int WindowMargin = 2;
        private int _indentationLevel;

        protected IConsole Console { get; }

        public int ColumnGutter { get; } 

        public int IndentationSize { get; } 

        public int MaxWidth { get; } 

        /// <summary>
        /// Brokers the generation and output of help text of <see cref="Symbol"/>
        /// and the <see cref="IConsole"/>
        /// </summary>
        /// <param name="console"><see cref="IConsole"/> instance to write the help text output</param>
        /// <param name="columnGutter">
        /// Number of characters to pad invocation information from their descriptions
        /// </param>
        /// <param name="indentationSize">Number of characters to indent new lines</param>
        /// <param name="maxWidth">
        /// Maximum number of characters available for each line to write to the console
        /// </param>
        public HelpBuilder(
            IConsole console,
            int? columnGutter = null,
            int? indentationSize = null,
            int? maxWidth = null)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
            ColumnGutter = columnGutter ?? DefaultColumnGutter;
            IndentationSize = indentationSize ?? DefaultIndentationSize;

            MaxWidth = maxWidth
                       ?? (Console is SystemConsole
                               ? GetConsoleWindowWidth()
                               : int.MaxValue);
        }

        /// <inheritdoc />
        public virtual void Write(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            AddSynopsis(command);
            AddUsage(command);
            AddArguments(command);
            AddOptions(command);
            AddSubcommands(command);
            AddAdditionalArguments(command);
        }

        protected int CurrentIndentation => _indentationLevel * IndentationSize;

        /// <summary>
        /// Increases the current indentation level
        /// </summary>
        protected void Indent(int levels = 1)
        {
            _indentationLevel += levels;
        }

        /// <summary>
        /// Decreases the current indentation level
        /// </summary>
        protected void Outdent(int levels = 1)
        {
            if (_indentationLevel == 0)
            {
                throw new InvalidOperationException("Cannot outdent any further");
            }

            _indentationLevel -= levels;
        }

        /// <summary>
        /// Gets the currently available space based on the <see cref="MaxWidth"/> from the window
        /// and the current indentation level
        /// </summary>
        /// <returns>The number of characters available on the current line</returns>
        protected int GetAvailableWidth()
        {
            return MaxWidth - CurrentIndentation - WindowMargin;
        }

        /// <summary>
        /// Create a string of whitespace for the supplied number of characters
        /// </summary>
        /// <param name="width">The length of whitespace required</param>
        /// <returns>A string of <see cref="width"/> whitespace characters</returns>
        protected static string GetPadding(int width)
        {
            return new string(' ', width);
        }

        /// <summary>
        /// Writes a blank line to the console
        /// </summary>
        private void AppendBlankLine()
        {
            Console.Out.WriteLine();
        }


        /// <summary>
        /// Writes whitespace to the console based on the provided offset,
        /// defaulting to the <see cref="CurrentIndentation"/>
        /// </summary>
        /// <param name="offset">Number of characters to pad</param>
        private void AppendPadding(int? offset = null)
        {
            var padding = GetPadding(offset ?? CurrentIndentation);
            Console.Out.Write(padding);
        }

        /// <summary>
        /// Writes a new line of text to the console, padded with a supplied offset
        /// defaulting to the <see cref="CurrentIndentation"/>
        /// </summary>
        /// <param name="text">The text content to write to the console</param>
        /// <param name="offset">Number of characters to pad the text</param>
        private void AppendLine(string text, int? offset = null)
        {
            AppendPadding(offset);
            Console.Out.WriteLine(text ?? "");
        }

        /// <summary>
        /// Writes text to the console, padded with a supplied offset
        /// </summary>
        /// <param name="text">Text content to write to the console</param>
        /// <param name="offset">Number of characters to pad the text</param>
        private void AppendText(string text, int? offset = null)
        {
            AppendPadding(offset);
            Console.Out.Write(text ?? "");
        }

        /// <summary>
        /// Writes heading text to the console.
        /// </summary>
        /// <param name="heading">Heading text content to write to the console</param>
        /// <exception cref="ArgumentNullException"></exception>
        private void AppendHeading(string heading)
        {
            if (heading == null)
            {
                throw new ArgumentNullException(nameof(heading));
            }

            AppendLine(heading);
        }

        /// <summary>
        /// Writes a description block to the console
        /// </summary>
        /// <param name="description">Description text to write to the console</param>
        /// <exception cref="ArgumentNullException"></exception>
        private void AppendDescription(string description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            var availableWidth = GetAvailableWidth();
            var descriptionLines = SplitText(description, availableWidth);

            foreach (var descriptionLine in descriptionLines)
            {
                AppendLine(descriptionLine, CurrentIndentation);
            }
        }

        /// <summary>
        /// Adds columnar content for a <see cref="HelpItem"/> using the current indentation
        /// for the line, and adding the appropriate padding between the columns
        /// </summary>
        /// <param name="helpItem">
        /// Current <see cref="HelpItem" /> to write to the console
        /// </param>
        /// <param name="maxInvocationWidth">
        /// Maximum number of characters accross all <see cref="HelpItem">help items</see>
        /// occupied by the invocation text
        /// </param>
        protected void AppendHelpItem(HelpItem helpItem, int maxInvocationWidth)
        {
            if (helpItem == null)
            {
                throw new ArgumentNullException(nameof(helpItem));
            }

            AppendText(helpItem.Invocation, CurrentIndentation);

            var offset = maxInvocationWidth + ColumnGutter - helpItem.Invocation.Length;
            var availableWidth = GetAvailableWidth();
            var maxDescriptionWidth = availableWidth - maxInvocationWidth - ColumnGutter;

            var descriptionLines = SplitText(helpItem.Description, maxDescriptionWidth);
            var lineCount = descriptionLines.Count;

            AppendLine(descriptionLines.FirstOrDefault(), offset);

            if (lineCount == 1)
            {
                return;
            }

            offset = CurrentIndentation + maxInvocationWidth + ColumnGutter;

            foreach (var descriptionLine in descriptionLines.Skip(1))
            {
                AppendLine(descriptionLine, offset);
            }
        }

        /// <summary>
        /// Takes a string of text and breaks it into lines of <see cref="maxLength"/>
        /// characters. This does not preserve any formatting of the incoming text.
        /// </summary>
        /// <param name="text">Text content to split into writable lines</param>
        /// <param name="maxLength">
        /// Maximum number of characters allowed for writing the supplied <see cref="text"/>
        /// </param>
        /// <returns>
        /// Collection of lines of at most <see cref="maxLength"/> characters
        /// generated from the supplied <see cref="text"/>
        /// </returns>
        protected virtual IReadOnlyCollection<string> SplitText(string text, int maxLength)
        {
            var cleanText = Regex.Replace(text, "\\s+", " ");
            var textLength = cleanText.Length;

            if (string.IsNullOrWhiteSpace(cleanText) || textLength < maxLength)
            {
                return new[] {cleanText};
            }

            var lines = new List<string>();
            var builder = new StringBuilder();

            foreach (var item in cleanText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries))
            {
                var length = item.Length + builder.Length;

                if (length >= maxLength)
                {
                    lines.Add(builder.ToString());
                    builder.Clear();
                }

                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }

                builder.Append(item);
            }

            if (builder.Length > 0)
            {
                lines.Add(builder.ToString());
            }

            return lines;
        }

        /// <summary>
        /// Formats the help rows for a given argument
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>A new <see cref="HelpItem"/></returns>
        private IEnumerable<HelpItem> GetArgumentHelpItems(ISymbol symbol)
        {
            foreach (var argument in symbol.Arguments())
            {
                if(ShouldShowHelp(argument))
                {
                    var argumentDescriptor = ArgumentDescriptor(argument);

                    var invocation = string.IsNullOrWhiteSpace(argumentDescriptor)
                                        ? ""
                                        : $"<{argumentDescriptor}>";

                    var argumentDescription = argument?.Description ?? "";
                
                    yield return new HelpItem(invocation, argumentDescription);
                }
            }
        }

        protected virtual string ArgumentDescriptor(IArgument argument)
        {
            if (argument.ValueType == typeof(bool) || argument.ValueType == typeof(bool?) )
            {
                return "";
            }

            var suggestions = argument.GetSuggestions().ToArray();
            if (suggestions.Length > 0)
            {
                return string.Join("|", suggestions);
            }

            return argument.Name;
        }

        /// <summary>
        /// Formats the help rows for a given option
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>A new <see cref="HelpItem"/></returns>
        private IEnumerable<HelpItem> GetOptionHelpItems(ISymbol symbol)
        {
            var rawAliases = symbol
                             .RawAliases
                             .Select(r => r.SplitPrefix())
                             .OrderBy(r => r.alias)
                             .ThenBy(r => r.prefix)
                             .GroupBy(t => t.alias)
                             .Select(t => t.First())
                             .Select(t => $"{t.prefix}{t.alias}");

            var invocation = string.Join(", ", rawAliases);

            if (ShouldShowHelp(symbol))
            {
                foreach (var argument in symbol.Arguments())
                {
                    if (ShouldShowHelp(argument) &&
                        !string.IsNullOrWhiteSpace(argument.Name))
                    {
                        var argumentDescriptor = ArgumentDescriptor(argument);
                        if (!string.IsNullOrWhiteSpace(argumentDescriptor))
                        {
                            invocation = $"{invocation} <{argumentDescriptor}>";
                        }
                    }
                }
            }

            if (symbol is IOption option &&
                option.Required)
            {
                invocation += " (REQUIRED)";
            }

            yield return new HelpItem(invocation, symbol.Description);
        }

        /// <summary>
        /// Writes a summary, if configured, for the supplied <see cref="command"/>
        /// </summary>
        /// <param name="command"></param>
        protected virtual void AddSynopsis(ICommand command)
        {
            if (!ShouldShowHelp(command))
            {
                return;
            }

            var title = $"{command.Name}:";
            HelpSection.Write(this, title, command.Description);
        }

        /// <summary>
        /// Writes the usage summary for the supplied <see cref="command"/>
        /// </summary>
        /// <param name="command"></param>
        protected virtual void AddUsage(ICommand command)
        {
            var usage = new List<string>();

            IEnumerable<ICommand> subcommands;

            if (command is Command cmd)
            {
                subcommands = cmd
                              .RecurseWhileNotNull(c => c.Parents
                                                         .OfType<Command>()
                                                         .FirstOrDefault())
                              .Reverse();
            }
            else
            {
                subcommands = Enumerable.Empty<ICommand>();
            }

            foreach (var subcommand in subcommands)
            {
                usage.Add(subcommand.Name);

                if (subcommand != command)
                {
                    usage.Add(FormatArgumentUsage(subcommand.Arguments.ToArray()));
                }
            }

            var hasOptionHelp = command.Children
                .OfType<IOption>()
                .Any(ShouldShowHelp);

            if (hasOptionHelp)
            {
                usage.Add(Usage.Options);
            }
            
            usage.Add(FormatArgumentUsage(command.Arguments.ToArray()));

            var hasCommandHelp = command.Children
                .OfType<ICommand>()
                .Any(ShouldShowHelp);

            if (hasCommandHelp)
            {
                usage.Add(Usage.Command);
            }

            if (!command.TreatUnmatchedTokensAsErrors)
            {
                usage.Add(Usage.AdditionalArguments);
            }

            HelpSection.Write(this, Usage.Title, string.Join(" ", usage.Where(u => !string.IsNullOrWhiteSpace(u))));
        }

        private string FormatArgumentUsage(IReadOnlyCollection<IArgument> arguments)
        {
            var sb = new StringBuilder();
            var args = new List<IArgument>(arguments.Where(ShouldShowHelp));
            var end = new Stack<string>();

            for (var i = 0; i < args.Count; i++)
            {
                var argument = args.ElementAt(i);

                var arityIndicator =
                    argument.Arity.MaximumNumberOfValues > 1
                        ? "..."
                        : "";

                var isOptional = IsOptional(argument);

                if (isOptional)
                {
                    sb.Append($"[<{argument.Name}>{arityIndicator}");
                }
                else
                {
                    sb.Append($"<{argument.Name}>{arityIndicator}");
                }

                if (i < args.Count - 1)
                {
                    sb.Append(" ");
                }

                if (isOptional)
                {
                    end.Push("]");
                }
            }

            while (end.Count > 0)
            {
                sb.Append(end.Pop());
            }

            return sb.ToString();

            bool IsMultiParented(IArgument argument) =>
                argument is Argument a &&
                a.Parents.Count > 1;

            bool IsOptional(IArgument argument) =>
                IsMultiParented(argument) ||
                argument.Arity.MinimumNumberOfValues == 0;
        }

        /// <summary>
        /// Writes the arguments, if any, for the supplied <see cref="command"/>
        /// </summary>
        /// <param name="command"></param>
        protected virtual void AddArguments(ICommand command)
        {
            var commands = new List<ICommand>();

            if (command is Command cmd &&
                cmd.Parents.FirstOrDefault() is ICommand parent &&
                ShouldDisplayArgumentHelp(parent))
            {
                commands.Add(parent);
            }

            if (ShouldDisplayArgumentHelp(command))
            {
                commands.Add(command);
            }

            HelpSection.Write(this, Arguments.Title, commands, GetArgumentHelpItems);
        }

        /// <summary>
        /// Writes the <see cref="Option"/> help content, if any,
        /// for the supplied <see cref="command"/>
        /// </summary>
        /// <param name="command"></param>
        protected virtual void AddOptions(ICommand command)
        {
            var options = command
                .Children
                .OfType<IOption>()
                .Where(ShouldShowHelp)
                .ToArray();

            HelpSection.Write(this, Options.Title, options, GetOptionHelpItems);
        }

        /// <summary>
        /// Writes the help content of the <see cref="Command"/> subcommands, if any,
        /// for the supplied <see cref="command"/>
        /// </summary>
        /// <param name="command"></param>
        protected virtual void AddSubcommands(ICommand command)
        {
            var subcommands = command
                .Children
                .OfType<ICommand>()
                .Where(ShouldShowHelp)
                .ToArray();

            HelpSection.Write(this, Commands.Title, subcommands, GetOptionHelpItems);
        }

        protected virtual void AddAdditionalArguments(ICommand command)
        {
            if (command.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            HelpSection.Write(this, AdditionalArguments.Title, AdditionalArguments.Description);
        }

        private bool ShouldDisplayArgumentHelp(ICommand command)
        {
            if (command == null)
            {
                return false;
            }

            return command.Arguments.Any(ShouldShowHelp);
        }

        private int GetConsoleWindowWidth()
        {
            try 
            {
                return System.Console.WindowWidth;
            }
            catch (System.IO.IOException)
            {
                return int.MaxValue;
            }             
        }

        protected class HelpItem
        {
            public HelpItem(string invocation, string description = null)
            {
                Invocation = invocation;
                Description = description ?? "";
            }

            public string Invocation { get; }

            public string Description { get; }

            protected bool Equals(HelpItem other) => 
                (Invocation, Description) == (other.Invocation, other.Description);

            public override bool Equals(object obj) => Equals((HelpItem) obj);

            public override int GetHashCode() => (Invocation, Description).GetHashCode();
        }

        private static class HelpSection
        {
            public static void Write(
                HelpBuilder builder,
                string title,
                string description = null)
            {
                if (!ShouldWrite(description, null))
                {
                    return;
                }

                AppendHeading(builder, title);
                builder.Indent();
                AddDescription(builder, description);
                builder.Outdent();
                builder.AppendBlankLine();
            }

            public static void Write(
                HelpBuilder builder,
                string title,
                IReadOnlyCollection<ISymbol> usageItems = null,
                Func<ISymbol, IEnumerable<HelpItem>> formatter = null,
                string description = null)
            {
                if (!ShouldWrite(description, usageItems))
                {
                    return;
                }

                AppendHeading(builder, title);
                builder.Indent();
                AddDescription(builder, description);
                AddInvocation(builder, usageItems, formatter);
                builder.Outdent();
                builder.AppendBlankLine();
            }

            private static bool ShouldWrite(string description, IReadOnlyCollection<ISymbol> usageItems)
            {
                if (!string.IsNullOrWhiteSpace(description))
                {
                    return true;
                }

                return usageItems?.Any() == true;
            }

            private static void AppendHeading(HelpBuilder builder, string title = null)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return;
                }

                builder.AppendHeading(title);
            }

            private static void AddDescription(HelpBuilder builder, string description = null)
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    return;
                }

                builder.AppendDescription(description);
            }

            private static void AddInvocation(
                HelpBuilder builder,
                IReadOnlyCollection<ISymbol> symbols,
                Func<ISymbol, IEnumerable<HelpItem>> formatter)
            {
                var helpItems = symbols
                    .SelectMany(formatter)
                    .Distinct()
                    .ToList();

                var maxWidth = helpItems
                    .Select(line => line.Invocation.Length)
                    .OrderByDescending(textLength => textLength)
                    .First();

                foreach (var helpItem in helpItems)
                {
                    builder.AppendHelpItem(helpItem, maxWidth);
                }
            }
        }

        internal bool ShouldShowHelp(ISymbol symbol)
        {
            return !symbol.IsHidden;
        }
    }
}
