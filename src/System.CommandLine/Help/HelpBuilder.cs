using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Environment;
using static System.CommandLine.DefaultHelpText;

namespace System.CommandLine
{
    public class HelpBuilder : IHelpBuilder
    {
        private readonly StringBuilder _helpText;
        private int _indentationLevel;

        internal const int DefaultColumnGutter = 4;
        internal const int DefaultIndentationSize = 2;
        internal const int DefaultWindowWidth = 80;
        internal const int WindowMargin = 2;

        public int ColumnGutter { get; private set; } = DefaultColumnGutter;
        public int IndentationSize { get; private set; } = DefaultIndentationSize;
        public int MaxWidth { get; private set; } = DefaultWindowWidth;

        public HelpBuilder()
        {
            _helpText = new StringBuilder();
        }

        /// <inheritdoc />
        public void Configure(int? columnGutter = null, int? indentationSize = null, int? maxWidth = null)
        {
            ColumnGutter = columnGutter ?? DefaultColumnGutter;
            IndentationSize = indentationSize ?? DefaultIndentationSize;
            MaxWidth = maxWidth ?? GetWindowWidth();
        }

        /// <inheritdoc />
        public void ResetConfiguration()
        {
            _indentationLevel = 0;
            ColumnGutter = DefaultColumnGutter;
            IndentationSize = DefaultIndentationSize;
            MaxWidth = DefaultWindowWidth;
        }

        /// <inheritdoc />
        public string Generate(CommandDefinition commandDefinition)
        {
            if (commandDefinition == null)
            {
                throw new ArgumentNullException(nameof(commandDefinition));
            }

            AddSynopsis(commandDefinition);
            AddArgumentsSection(commandDefinition);
            AddOptionsSection(commandDefinition);
            AddSubcommandsSection(commandDefinition);
            AddAdditionalArgumentsSection(commandDefinition);
            return _helpText.ToString();
        }

        internal int CurrentIndentation => _indentationLevel * IndentationSize;

        /// <summary>
        /// Increases the current indentation level
        /// </summary>
        internal void Indent()
        {
            _indentationLevel += 1;
        }

        /// <summary>
        /// Decreases the current indentation level
        /// </summary>
        internal void Outdent()
        {
            if (_indentationLevel == 0)
            {
                throw new ArithmeticException("Cannot dedent any further");
            }

            _indentationLevel -= 1;
        }

        /// <summary>
        /// Gets the currently available width based on the <see cref="MaxWidth"/> from the window
        /// and the current indentation level
        /// </summary>
        /// <returns></returns>
        internal int GetAvailableWidth()
        {
            return MaxWidth - CurrentIndentation - WindowMargin;
        }

        /// <summary>
        /// Create a string of whitespace for the supplied number of characters
        /// </summary>
        /// <param name="width"></param>
        internal static string GetPadding(int width)
        {
            return new string(' ', width);
        }

        /// <summary>
        /// Adds a blank line to the current builder
        /// </summary>
        internal void AddBlankLine()
        {
            _helpText.AppendLine();
        }

        /// <summary>
        /// Adds a new line of text to the current builder, padded with the current indentation
        /// </summary>
        /// <param name="text"></param>
        internal void AddLine(string text)
        {
            _helpText.AppendFormat(
                "{0}{1}{2}",
                GetPadding(CurrentIndentation),
                text ?? "",
                NewLine);
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

            foreach (var item in cleanText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
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
        /// Adds columnar content for a <see cref="HelpDefinition"/> using the current indentation
        /// for the line, and adding the appropriate padding between the columns
        /// </summary>
        /// <param name="leftColumn"></param>
        /// <param name="rightColumn"></param>
        /// <param name="maxLeftColumnWidth"></param>
        internal virtual void AddSectionColumns(string leftColumn, string rightColumn, int maxLeftColumnWidth)
        {
            var availableWidth = GetAvailableWidth();
            var offset = maxLeftColumnWidth + ColumnGutter - leftColumn.Length;

            var maxRightColumnWidth = availableWidth - maxLeftColumnWidth - ColumnGutter;
            var rightColumnLines = SplitText(rightColumn, maxRightColumnWidth);

            _helpText.AppendFormat(
                "{0}{1}{2}{3}{4}",
                GetPadding(CurrentIndentation),
                leftColumn,
                GetPadding(offset),
                rightColumnLines.First(),
                NewLine);

            offset = CurrentIndentation + maxLeftColumnWidth + ColumnGutter;
            foreach (var line in rightColumnLines.Skip(1))
            {
                _helpText.AppendFormat(
                    "{0}{1}",
                    GetPadding(offset),
                    line);
            }
        }

        /// <summary>
        /// Formats the help rows for a given argument
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected static Tuple<string, string> ArgumentFormatter(SymbolDefinition symbol)
        {
            var argHelp = symbol?.ArgumentDefinition?.Help;

            var leftColumn = $"<{argHelp?.Name}>";
            var rightColumn = argHelp?.Description ?? "";
            return Tuple.Create(leftColumn, rightColumn);
        }

        /// <summary>
        /// Formats the help rows for a given option
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected static Tuple<string, string> OptionFormatter(SymbolDefinition symbol)
        {
            var rawAliases = symbol.RawAliases
                .OrderBy(alias => alias.Length);

            var aliases = string.Join(", ", rawAliases);

            var leftColumn = symbol.HasArguments
                ? $"{aliases} <{symbol.ArgumentDefinition?.Help?.Name}>"
                : aliases;

            var rightColumn = symbol.Help?.Description ?? "";
            return Tuple.Create(leftColumn, rightColumn);
        }

        protected virtual void AddSynopsis(CommandDefinition commandDefinition)
        {
            _helpText.Append(Synopsis.Title);

            var subcommands = commandDefinition
                .RecurseWhileNotNull(commandDef => commandDef.Parent)
                .Reverse();

            foreach (var subcommand in subcommands)
            {
                _helpText.AppendFormat(" {0}", subcommand.Name);

                var argsName = subcommand.ArgumentDefinition?.Help?.Name;
                if (subcommand != commandDefinition && !string.IsNullOrWhiteSpace(argsName))
                {
                    _helpText.AppendFormat(" <{0}>", argsName);
                }
            }

            var hasOptionHelp = commandDefinition.SymbolDefinitions
                .OfType<OptionDefinition>()
                .Any(symbolDef => symbolDef.HasHelp);

            if (hasOptionHelp)
            {
                _helpText.AppendFormat(" {0}", Synopsis.Options);
            }

            var argumentsName = commandDefinition.ArgumentDefinition?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                _helpText.AppendFormat(" <{0}>", argumentsName);
            }

            var hasCommand = commandDefinition.SymbolDefinitions
                .OfType<CommandDefinition>()
                .Any();

            if (hasCommand)
            {
                _helpText.AppendFormat(" {0}", Synopsis.Command);
            }

            if (!commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                _helpText.AppendFormat(" {0}", Synopsis.AdditionalArguments);
            }

            AddBlankLine();
        }

        protected virtual void AddArgumentsSection(CommandDefinition commandDefinition)
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

            var section = new HelpSection(this, ArgumentsSection.Title, arguments, ArgumentFormatter);
            section.Build();
        }

        private void AddOptionsSection(SymbolDefinition commandDefinition)
        {
            var options = commandDefinition
                .SymbolDefinitions
                .OfType<OptionDefinition>()
                .Where(opt => opt.HasHelp)
                .ToArray();

            var section = new HelpSection(this, OptionsSection.Title, options, OptionFormatter);
            section.Build();
        }

        protected virtual void AddSubcommandsSection(SymbolDefinition commandDefinition)
        {
            var subcommands = commandDefinition
                .SymbolDefinitions
                .OfType<CommandDefinition>()
                .Where(subCommand => subCommand.HasHelp)
                .ToArray();

            var section = new HelpSection(this, CommandsSection.Title, subcommands, OptionFormatter);
            section.Build();
        }

        protected virtual void AddAdditionalArgumentsSection(CommandDefinition commandDefinition)
        {
            if (commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                return;
            }

            var section = new HelpSection(this, AdditionalArgumentsSection.Title, AdditionalArgumentsSection.Description);
            section.Build();
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
            catch (Exception exception)
            {
                if (exception is ArgumentOutOfRangeException || exception is IOException)
                {
                    return DefaultWindowWidth;
                }

                throw;
            }
        }
    }
}
