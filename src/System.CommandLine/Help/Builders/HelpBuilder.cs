using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.CommandLine.DefaultHelpText;

namespace System.CommandLine
{
    public class HelpBuilder : IHelpBuilder
    {
        private int _indentationLevel;
        protected IConsole _console;

        protected const int DefaultColumnGutter = 4;
        protected const int DefaultIndentationSize = 2;
        protected const int DefaultWindowWidth = 80;
        protected const int WindowMargin = 2;

        public int ColumnGutter { get; } = DefaultColumnGutter;
        public int IndentationSize { get; } = DefaultIndentationSize;
        public int MaxWidth { get; } = DefaultWindowWidth;

        /// <summary>
        ///
        /// </summary>
        /// <param name="console"></param>
        /// <param name="columnGutter"></param>
        /// <param name="indentationSize"></param>
        /// <param name="maxWidth"></param>
        public HelpBuilder(
            IConsole console,
            int? columnGutter = null,
            int? indentationSize = null,
            int? maxWidth = null)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            ColumnGutter = columnGutter ?? DefaultColumnGutter;
            IndentationSize = indentationSize ?? DefaultIndentationSize;
            MaxWidth = maxWidth ?? GetWindowWidth();
        }

        /// <inheritdoc />
        public void Generate(CommandDefinition commandDefinition)
        {
            if (commandDefinition == null)
            {
                throw new ArgumentNullException(nameof(commandDefinition));
            }

            AddSynopsis(commandDefinition);
            AddArguments(commandDefinition);
            AddOptions(commandDefinition);
            AddSubcommands(commandDefinition);
            AddAdditionalArguments(commandDefinition);
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
                throw new ArithmeticException("Cannot dedent any further");
            }

            _indentationLevel -= levels;
        }

        /// <summary>
        /// Gets the currently available width based on the <see cref="MaxWidth"/> from the window
        /// and the current indentation level
        /// </summary>
        /// <returns></returns>
        protected int GetAvailableWidth()
        {
            return MaxWidth - CurrentIndentation - WindowMargin;
        }

        /// <summary>
        /// Create a string of whitespace for the supplied number of characters
        /// </summary>
        /// <param name="width"></param>
        protected static string GetPadding(int width)
        {
            return new string(' ', width);
        }

        /// <summary>
        /// Adds a blank line to the current builder
        /// </summary>
        private void AppendBlankLine()
        {
            _console.Out.WriteLine();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        private void AppendPadding(int? offset = null)
        {
            var padding = GetPadding(offset ?? CurrentIndentation);
            _console.Out.Write(padding);
        }

        /// <summary>
        /// Adds a new line of text to the current builder, padded with the current indentation
        /// </summary>
        /// <param name="text"></param>
        /// /// <param name="offset"></param>
        private void AppendLine(string text, int? offset = null)
        {
            AppendPadding(offset);
            _console.Out.WriteLine(text ?? "");
        }

        /// <summary>
        /// Adds a new line of text to the current builder, padded with the current indentation
        /// </summary>
        /// <param name="text"></param>
        /// <param name="offset"></param>
        private void AppendText(string text, int? offset = null)
        {
            AppendPadding(offset);
            _console.Out.Write(text ?? "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heading"></param>
        protected virtual void AddHeading(string heading)
        {
            AppendLine(heading);
        }

        /// <summary>
        /// Adds columnar content for a <see cref="HelpDefinition"/> using the current indentation
        /// for the line, and adding the appropriate padding between the columns
        /// </summary>
        /// <param name="helpItem"></param>
        /// <param name="maxLeftColumnWidth"></param>
        protected void AddHelpItem(HelpItem helpItem, int maxLeftColumnWidth)
        {
            if (helpItem == null)
            {
                throw new ArgumentNullException(nameof(helpItem));
            }

            AppendText(helpItem.Usage, CurrentIndentation);

            var offset = maxLeftColumnWidth + ColumnGutter - helpItem.Usage.Length;
            var availableWidth = GetAvailableWidth();
            var maxRightColumnWidth = availableWidth - maxLeftColumnWidth - ColumnGutter;

            var descriptionLines = SplitText(helpItem.Description, maxRightColumnWidth);
            var lineCount = descriptionLines.Count;

            AppendLine(descriptionLines.First(), offset);

            if (lineCount == 1)
            {
                return;
            }

            offset = CurrentIndentation + maxLeftColumnWidth + ColumnGutter;

            for (var i = 1; i < lineCount; i++)
            {
                AppendLine(descriptionLines.ElementAt(i), offset);
            }
        }

        /// <summary>
        /// Takes a string of text and breaks it into lines of <see cref="maxLength"/>
        /// characters. This does not preserve any formatting of the incoming text - something
        /// that would need to be handled on a derivation of HelpBuilder
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
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
            var index = 0;

            foreach (var item in cleanText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries))
            {
                var length = item.Length + builder.Length;

                if (length > maxLength)
                {
                    lines.Add(builder.ToString());
                    builder.Clear();
                    index = 0;
                }

                if (index != 0)
                {
                    builder.Append(" ");
                }

                builder.Append(item);
                index += 1;
            }

            if (index != 0)
            {
                lines.Add(builder.ToString());
            }

            return lines;
        }

        /// <summary>
        /// Formats the help rows for a given argument
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected virtual HelpItem ArgumentFormatter(SymbolDefinition symbol)
        {
            var argHelp = symbol?.ArgumentDefinition?.Help;

            return new HelpItem {
                Usage = $"<{argHelp?.Name}>",
                Description = argHelp?.Description ?? "",
            };
        }

        /// <summary>
        /// Formats the help rows for a given option
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected virtual HelpItem OptionFormatter(SymbolDefinition symbol)
        {
            var rawAliases = symbol.RawAliases
                .OrderBy(alias => alias.Length);

            var usage = string.Join(", ", rawAliases);

            if (symbol.HasArguments && !string.IsNullOrWhiteSpace(symbol.ArgumentDefinition?.Help?.Name))
            {
                usage = $"{usage} <{symbol.ArgumentDefinition?.Help?.Name}>";
            }

            return new HelpItem {
                Usage = usage,
                Description = symbol.Help?.Description ?? "",
            };
        }

        protected virtual void AddSynopsis(CommandDefinition commandDefinition)
        {
            var synopsis = new StringBuilder();

            var subcommands = commandDefinition
                .RecurseWhileNotNull(commandDef => commandDef.Parent)
                .Reverse();

            foreach (var subcommand in subcommands)
            {
                synopsis.AppendFormat("{0} ", subcommand.Name);

                var argsName = subcommand.ArgumentDefinition?.Help?.Name;
                if (subcommand != commandDefinition && !string.IsNullOrWhiteSpace(argsName))
                {
                    synopsis.AppendFormat("<{0}> ", argsName);
                }
            }

            var hasOptionHelp = commandDefinition.SymbolDefinitions
                .OfType<OptionDefinition>()
                .Any(symbolDef => symbolDef.HasHelp);

            if (hasOptionHelp)
            {
                synopsis.AppendFormat("{0} ", Synopsis.Options);
            }

            var argumentsName = commandDefinition.ArgumentDefinition?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                synopsis.AppendFormat("<{0}> ", argumentsName);
            }

            var hasCommand = commandDefinition.SymbolDefinitions
                .OfType<CommandDefinition>()
                .Any();

            if (hasCommand)
            {
                synopsis.AppendFormat("{0} ", Synopsis.Command);
            }

            if (!commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                synopsis.AppendFormat("{0} ", Synopsis.AdditionalArguments);
            }

            HelpSection.Build(this, Synopsis.Title, synopsis.ToString());
        }

        protected virtual void AddArguments(CommandDefinition commandDefinition)
        {
            var arguments = new List<CommandDefinition>();

            if (commandDefinition.Parent?.HasArguments == true && commandDefinition.Parent.HasHelp)
            {
                arguments.Add(commandDefinition.Parent);
            }

            if (commandDefinition.HasArguments && commandDefinition.HasHelp)
            {
                arguments.Add(commandDefinition);
            }

            HelpSection.Build(this, Arguments.Title, arguments, ArgumentFormatter);
        }

        protected virtual void AddOptions(SymbolDefinition commandDefinition)
        {
            var options = commandDefinition
                .SymbolDefinitions
                .OfType<OptionDefinition>()
                .Where(opt => opt.HasHelp)
                .ToArray();

            HelpSection.Build(this, Options.Title, options, OptionFormatter);
        }

        protected virtual void AddSubcommands(SymbolDefinition commandDefinition)
        {
            var subcommands = commandDefinition
                .SymbolDefinitions
                .OfType<CommandDefinition>()
                .Where(subCommand => subCommand.HasHelp)
                .ToArray();

            HelpSection.Build(this, Commands.Title, subcommands, OptionFormatter);
        }

        protected virtual void AddAdditionalArguments(CommandDefinition commandDefinition)
        {
            if (commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            HelpSection.Build(this, AdditionalArguments.Title, AdditionalArguments.Description);
        }

        /// <summary>
        /// Gets the width of the current window, falling back to the <see cref="DefaultWindowWidth"/>
        /// if necessary
        /// </summary>
        /// <returns></returns>
        private static int GetWindowWidth()
        {
            try
            {
                return Console.WindowWidth;
            }
            catch (Exception exception) when (exception is ArgumentOutOfRangeException || exception is IOException)
            {
                return DefaultWindowWidth;
            }
        }

        protected class HelpItem
        {
            public string Usage { get; set; }

            public string Description { get; set; }
        }

        private static class HelpSection
        {
            public static void Build(
                HelpBuilder builder,
                string title,
                IReadOnlyCollection<SymbolDefinition> usageItems = null,
                Func<SymbolDefinition, HelpItem> formatter = null,
                string description = null)
            {
                if (!ShouldBuild(description, usageItems))
                {
                    return;
                }

                AddHeading(builder, title);
                builder.Indent();
                AddDescription(builder, description);
                AddUsage(builder, usageItems, formatter);
                builder.Outdent();
            }

            public static void Build(
                HelpBuilder builder,
                string title,
                string description)
            {
                if (!ShouldBuild(description, null))
                {
                    return;
                }

                AddHeading(builder, title);
                builder.Indent();
                AddDescription(builder, description);
                builder.Outdent();
            }

            private static bool ShouldBuild(string description, IReadOnlyCollection<SymbolDefinition> usageItems)
            {
                if (!string.IsNullOrWhiteSpace(description))
                {
                    return true;
                }

                return usageItems?.Any() == true;
            }

            private static void AddHeading(HelpBuilder builder, string title)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return;
                }

                builder.AddHeading(title);
            }

            private static void AddDescription(HelpBuilder builder, string description)
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    return;
                }

                builder.AppendLine(description);
                builder.AppendBlankLine();
            }

            private static void AddUsage(
                HelpBuilder builder,
                IReadOnlyCollection<SymbolDefinition> usageItems,
                Func<SymbolDefinition, HelpItem> formatter)
            {
                if (usageItems?.Any() != true)
                {
                    return;
                }

                var helpItems = usageItems
                    .Select(item => formatter(item));

                var maxWidth = helpItems
                    .Select(line => line.Usage.Length)
                    .OrderByDescending(textLength => textLength)
                    .First();

                foreach (var helpItem in helpItems)
                {
                    builder.AddHelpItem(helpItem, maxWidth);
                }

                builder.AppendBlankLine();
            }
        }
    }
}
