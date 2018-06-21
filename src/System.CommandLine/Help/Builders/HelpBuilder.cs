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
        /// <param name="columnGutter"></param>
        /// <param name="indentationSize"></param>
        /// <param name="maxWidth"></param>
        public HelpBuilder(
            int? columnGutter = null,
            int? indentationSize = null,
            int? maxWidth = null)
        {
            ColumnGutter = columnGutter ?? DefaultColumnGutter;
            IndentationSize = indentationSize ?? DefaultIndentationSize;
            MaxWidth = maxWidth ?? GetWindowWidth();

            _helpText = new StringBuilder();
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

        protected int CurrentIndentation => _indentationLevel * IndentationSize;

        /// <summary>
        /// Increases the current indentation level
        /// </summary>
        public void Indent()
        {
            _indentationLevel += 1;
        }

        /// <summary>
        /// Decreases the current indentation level
        /// </summary>
        public void Outdent()
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
        public void AddBlankLine()
        {
            _helpText.AppendLine();
        }

        /// <summary>
        /// Adds a new line of text to the current builder, padded with the current indentation
        /// </summary>
        /// <param name="text"></param>
        public void AddLine(string text)
        {
            _helpText.AppendFormat(
                "{0}{1}{2}",
                GetPadding(CurrentIndentation),
                text ?? "",
                NewLine);
        }

        public void AddHeading(string heading)
        {
            _helpText.Append(heading);
        }

        /// <summary>
        /// Adds columnar content for a <see cref="HelpDefinition"/> using the current indentation
        /// for the line, and adding the appropriate padding between the columns
        /// </summary>
        /// <param name="helpItem"></param>
        /// <param name="maxLeftColumnWidth"></param>
        public void AddHelpItem(HelpItem helpItem, int maxLeftColumnWidth)
        {
            if (helpItem == null)
            {
                throw new ArgumentNullException(nameof(helpItem));
            }

            var availableWidth = GetAvailableWidth();
            var offset = maxLeftColumnWidth + ColumnGutter - helpItem.Usage.Length;

            var maxRightColumnWidth = availableWidth - maxLeftColumnWidth - ColumnGutter;
            var descriptionLines = SplitText(helpItem.Description, maxRightColumnWidth);
            var lineCount = descriptionLines.Count;

            _helpText.AppendFormat(
                "{0}{1}{2}{3}{4}",
                GetPadding(CurrentIndentation),
                helpItem.Usage,
                GetPadding(offset),
                descriptionLines.First(),
                NewLine);

            if (lineCount == 1)
            {
                return;
            }

            offset = CurrentIndentation + maxLeftColumnWidth + ColumnGutter;
            var paddedOffset = GetPadding(offset);

            for (var i = 1; i < lineCount; i++)
            {
                _helpText.AppendFormat(
                    "{0}{1}",
                    paddedOffset,
                    descriptionLines.ElementAt(i));

                if (i < lineCount - 1)
                {
                    _helpText.AppendLine();
                }
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
        protected static HelpItem ArgumentFormatter(SymbolDefinition symbol)
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
        protected static HelpItem OptionFormatter(SymbolDefinition symbol)
        {
            var rawAliases = symbol.RawAliases
                .OrderBy(alias => alias.Length);

            var aliases = string.Join(", ", rawAliases);

            var usage = symbol.HasArguments
                ? $"{aliases} <{symbol.ArgumentDefinition?.Help?.Name}>"
                : aliases;

            return new HelpItem {
                Usage = usage,
                Description = symbol.Help?.Description ?? "",
            };
        }

        protected virtual void AddSynopsis(CommandDefinition commandDefinition)
        {
            AddHeading(Synopsis.Title);

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

        protected virtual void AddOptionsSection(SymbolDefinition commandDefinition)
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
            catch (Exception exception) when (exception is ArgumentOutOfRangeException || exception is IOException)
            {
                return DefaultWindowWidth;
            }
        }
    }
}
