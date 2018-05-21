using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Environment;
using static System.CommandLine.DefaultHelpText;

namespace System.CommandLine
{
    public class HelpBuilder
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

        /// <summary>
        /// Resets the configuration properties to their default values
        /// </summary>
        public void ResetConfiguration()
        {
            _indentationLevel = 0;
            ColumnGutter = DefaultColumnGutter;
            IndentationSize = DefaultIndentationSize;
            MaxWidth = DefaultWindowWidth;
        }

        /// <summary>
        /// Sets the configuration properties to the specified, or default, values
        /// </summary>
        /// <param name="columnGutter"></param>
        /// /// <param name="indentationSize"></param>
        /// <param name="maxWidth"></param>
        public void Configure(int? columnGutter = null, int? indentationSize = null, int? maxWidth = null)
        {
            ColumnGutter = columnGutter ?? DefaultColumnGutter;
            IndentationSize = indentationSize ?? DefaultIndentationSize;
            MaxWidth = maxWidth ?? GetWindowWidth();
        }

        internal int CurrentIndentation => _indentationLevel * IndentationSize;

        internal void Indent()
        {
            _indentationLevel += 1;
        }

        internal void Dedent()
        {
            if (_indentationLevel == 0)
            {
                throw new ArithmeticException("Cannot dedent any further");
            }

            _indentationLevel -= 1;
        }

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
        /// Adds whitespace for the supplied number of characters
        /// </summary>
        /// <param name="width"></param>
        internal void AddPadding(int width)
        {
            _helpText.Append(new string(' ', width));
        }

        internal void AddBlankLine()
        {
            _helpText.AppendLine();
        }

        /// <summary>
        /// Returns the length of a given <see cref="HelpDefinition"/> Name
        /// property if it exists, returning 0 otherwise
        /// </summary>
        /// <param name="help"></param>
        /// <returns></returns>
        private static int GetHelpNameLength(HelpDefinition help)
        {
            return help?.Name?.Length ?? 0;
        }

        /// <summary>
        /// Adds a section of text to the current builder, padded with the current indentation
        /// </summary>
        /// <param name="text"></param>
        internal void AddText(string text)
        {
            _helpText.AppendFormat(
                "{0}{1}",
                new string(' ', CurrentIndentation),
                text ?? "");
        }

        /// <summary>
        /// Adds a new line of text to the current builder, padded with the current indentation
        /// </summary>
        /// <param name="text"></param>
        internal void AddLine(string text)
        {
            _helpText.AppendFormat(
                "{0}{1}{2}",
                new string(' ', CurrentIndentation),
                text ?? "",
                NewLine);
        }

        /// <summary>
        /// Adds columnar content for a <see cref="HelpDefinition"/> using the current indentation
        /// for the line, and adding the appropriate padding between the columns
        /// </summary>
        /// <param name="name"></param>
        /// <param name="padding"></param>
        /// <param name="description"></param>
        internal void AddSectionColumns(string name, int padding, string description)
        {
            _helpText.AppendFormat(
                "{0}{1}{2}{3}{4}",
                new string(' ', CurrentIndentation),
                name ?? "",
                new string(' ', padding),
                description ?? "",
                NewLine);
        }

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
                var cleanedItem = item.Trim();
                var length = cleanedItem.Length + builder.Length;

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
                builder.Append(cleanedItem);
                index += 1;
            }

            if (index != 0)
            {
                lines.Add(builder.ToString());
            }

            return lines;
        }

        internal void AddSectionColumns(string leftColumn, string rightColumn, int maxLeftColumnWidth)
        {
            var availableWidth = GetAvailableWidth();
            var offset = maxLeftColumnWidth + ColumnGutter - leftColumn.Length;

            var maxRightColumnWidth = availableWidth - maxLeftColumnWidth - ColumnGutter;
            var rightColumnLines = SplitText(rightColumn, maxRightColumnWidth);

            _helpText.AppendFormat(
                "{0}{1}{2}{3}{4}",
                new string(' ', CurrentIndentation),
                leftColumn,
                new string(' ', offset),
                rightColumnLines.First(),
                NewLine);

            offset = CurrentIndentation + maxLeftColumnWidth + ColumnGutter;
            foreach (var line in rightColumnLines.Skip(1))
            {
                _helpText.AppendFormat(
                    "{0}{1}",
                    new string(' ', offset),
                    line);
            }
        }

        public string Build(CommandDefinition commandDefinition)
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

        private void WriteColumns(string name, string description, int maxWidth)
        {
            if (name == null)
            {
                name = "";
            }

            if (description == null)
            {
                description = "";
            }

            var leftColumnWidth = ColumnGutter + CurrentIndentation + maxWidth;

            AddText(name);

            if (name.Length <= leftColumnWidth)
            {
                AddPadding(leftColumnWidth - name.Length - CurrentIndentation);
            }
            else
            {
                AddBlankLine();
                AddPadding(leftColumnWidth);
            }

            var descriptionWithLineWraps = string.Join(
                NewLine + new string(' ', leftColumnWidth),
                description
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()));

            _helpText.AppendLine(descriptionWithLineWraps);
        }

        private static string OptionFormatter(SymbolDefinition symbolDefinition)
        {
            var rawAliases = symbolDefinition.RawAliases
                .OrderBy(alias => alias.Length);

            var aliases = string.Join(", ", rawAliases);

            var argumentName = symbolDefinition.ArgumentDefinition?.Help?.Name;

            return string.IsNullOrWhiteSpace(argumentName)
                ? aliases
                : $"{aliases} <{argumentName}>";
        }

        private static Tuple<string, string> OptionFormatter2(SymbolDefinition symbol)
        {
            var rawAliases = symbol.RawAliases
                .OrderBy(alias => alias.Length);

            var aliases = string.Join(", ", rawAliases);

            var argumentName = symbol.ArgumentDefinition?.Help?.Name;

            var leftColumn = string.IsNullOrWhiteSpace(argumentName) ? aliases : $"{aliases} <{argumentName}>";
            var rightColumn = symbol.Description ?? "";
            return Tuple.Create(leftColumn, rightColumn);
        }

        private void WriteOptionsList(IReadOnlyCollection<SymbolDefinition> symbols)
        {
            var leftColumnTextFor = symbols.ToDictionary(symbol => symbol, OptionFormatter);

            var maxWidth = leftColumnTextFor.Values
                .Select(symbol => symbol.Length)
                .OrderByDescending(length => length)
                .First();

            foreach (var symbol in symbols)
            {
                WriteColumns(leftColumnTextFor[symbol], symbol.Description, maxWidth);
            }
        }

        private void AddSynopsis(CommandDefinition commandDefinition)
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

            var hasCommand = commandDefinition.SymbolDefinitions
                .OfType<CommandDefinition>()
                .Any();

            if (hasOptionHelp)
            {
                _helpText.AppendFormat(" {0}", Synopsis.Options);
            }

            var argumentsName = commandDefinition.ArgumentDefinition?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                _helpText.AppendFormat(" <{0}>", argumentsName);
            }

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

        private void AddArgumentsSection(CommandDefinition commandDefinition)
        {
            var showArgHelp = commandDefinition.HasArguments && commandDefinition.HasHelp;
            var showParentArgHelp = false;

            if (commandDefinition.Parent != null)
            {
                showParentArgHelp = commandDefinition.Parent.HasArguments && commandDefinition.Parent.HasHelp;
            }

            if (!showArgHelp && !showParentArgHelp)
            {
                return;
            }

            var argHelp = commandDefinition.ArgumentDefinition?.Help;
            var parentArgHelp = commandDefinition.Parent?.ArgumentDefinition?.Help;

            AddBlankLine();
            _helpText.AppendLine(ArgumentsSection.Title);

            Indent();

            var maxWidth = showArgHelp ? GetHelpNameLength(argHelp) : 0;

            if (showParentArgHelp)
            {
                maxWidth = Math.Max(maxWidth, GetHelpNameLength(parentArgHelp));
            }

            maxWidth += 2;

            if (showParentArgHelp)
            {
                WriteColumns($"<{parentArgHelp.Name}>", parentArgHelp.Description, maxWidth);
            }

            if (showArgHelp)
            {
                WriteColumns($"<{argHelp.Name}>", argHelp.Description, maxWidth);
            }

            Dedent();
        }

        private void AddOptionsSection(CommandDefinition commandDefinition)
        {
            var options = commandDefinition
                .SymbolDefinitions
                .OfType<OptionDefinition>()
                .Where(opt => opt.HasHelp)
                .ToArray();

            if (!options.Any())
            {
                return;
            }

            var section = new HelpSection(this, OptionsSection.Title, options, OptionFormatter2);
            section.Build();
        }

        private void AddSubcommandsSection(CommandDefinition commandDefinition)
        {
            var subcommands = commandDefinition
                .SymbolDefinitions
                .OfType<CommandDefinition>()
                .Where(subCommand => subCommand.HasHelp)
                .ToArray();

            if (!subcommands.Any())
            {
                return;
            }

            AddBlankLine();
            _helpText.AppendLine(CommandsSection.Title);

            Indent();
            WriteOptionsList(subcommands);
            Dedent();
        }

        private void AddAdditionalArgumentsSection(CommandDefinition commandDefinition)
        {
            if (commandDefinition?.TreatUnmatchedTokensAsErrors == true)
            {
                return;
            }

            _helpText.Append(AdditionalArgumentsSection.Title);
            AddBlankLine();
            Indent();
            _helpText.Append(AdditionalArgumentsSection.Description);
            Dedent();
        }
    }
}
