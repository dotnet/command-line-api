using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Environment;
using static System.CommandLine.DefaultHelpViewText;

namespace System.CommandLine
{
    public class HelpBuilder
    {
        private const int ColumnGutterWidth = 3;
        private const int DefaultWindowWidth = 80;
        private const int IndentationSize = 2;
        private const int MaxWidthLeeWay = 2;

        private readonly StringBuilder _builder;
        private int _indentationLevel;
        private int _currentIndentation;

        protected int MaxWidth { get; private set; }

        public HelpBuilder()
        {
            _builder = new StringBuilder();
        }

        internal int CurrentIndentation => _currentIndentation;

        private void Reset()
        {
            _currentIndentation = 0;
            _indentationLevel = 0;
        }

        internal void Indent()
        {
            _indentationLevel += 1;
            _currentIndentation += IndentationSize;
        }

        internal void Dedent()
        {
            if (_indentationLevel == 0)
            {
                throw new ArithmeticException("Cannot dedent any further");
            }

            _indentationLevel -= 1;
            _currentIndentation += IndentationSize;
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
            return MaxWidth - _currentIndentation - MaxWidthLeeWay;
        }

        /// <summary>
        /// Adds whitespace for the supplied number of characters
        /// </summary>
        /// <param name="width"></param>
        internal void AddPadding(int width)
        {
            _builder.Append(new string(' ', width));
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
        /// Adds a new line of text to the current builder, padded with the current indentation
        /// </summary>
        /// <param name="text"></param>
        internal void AddLine(string text)
        {
            _builder.AppendFormat(
                "{0}{1}{2}",
                new string(' ', _currentIndentation),
                text,
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
            _builder.AppendFormat(
                "{0}{1}{2}{3}{4}",
                new string(' ', _currentIndentation),
                name ?? "",
                new string(' ', padding),
                description ?? "",
                NewLine);
        }

        public string Build(CommandDefinition commandDefinition, int? maxWidth = null)
        {
            if (commandDefinition == null)
            {
                throw new ArgumentNullException(nameof(commandDefinition));
            }

            MaxWidth = maxWidth ?? GetWindowWidth();

            AddSynopsis(commandDefinition);
            AddArgumentsSection(commandDefinition);
            AddOptionsSection(commandDefinition);
            AddSubcommandsSection(commandDefinition);
            AddAdditionalArgumentsSection(commandDefinition);
            return _builder.ToString();
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

            AddIndentedText(name);

            if (name.Length <= maxWidth)
            {
                AddPadding(maxWidth - name.Length);
            }
            else
            {
                _builder.AppendLine();
                AddPadding(maxWidth);
            }

            var descriptionWithLineWraps = string.Join(
                NewLine + new string(' ', maxWidth),
                description
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()));

            _builder.AppendLine(descriptionWithLineWraps);
        }

        private static string OptionFormatter(SymbolDefinition symbolDefinition)
        {
            var aliases = string.Join(", ", symbolDefinition.RawAliases.OrderBy(alias => alias.Length));

            var argumentName = symbolDefinition.ArgumentDefinition?.Help?.Name;

            return string.IsNullOrWhiteSpace(argumentName)
                ? aliases
                : $"{aliases} <{argumentName}>";
        }

        private void WriteOptionsList(IReadOnlyCollection<SymbolDefinition> symbols)
        {
            var leftColumnTextFor = symbols.ToDictionary(symbol => symbol, OptionFormatter);

            var maxWidth = leftColumnTextFor
                .Values
                .Select(symbol => symbol.Length)
                .OrderByDescending(length => length)
                .First();

            var leftColumnWidth = ColumnGutterWidth + maxWidth;

            foreach (var symbol in symbols)
            {
                WriteColumns(leftColumnTextFor[symbol], symbol.Description, leftColumnWidth);
            }
        }

        private void AddSynopsis(CommandDefinition commandDefinition)
        {
            _builder.Append(Synopsis.Title);

            _builder.AppendFormat(" {0}", commandDefinition.Name);

            var subcommands = commandDefinition
                .RecurseWhileNotNull(commandDef => commandDef.Parent)
                .Where(commandDef => commandDef != commandDefinition)
                .Reverse();

            foreach (var subcommand in subcommands)
            {
                _builder.AppendFormat(" {0}", subcommand.Name);

                var argsName = subcommand.Help?.Name;
                if (!string.IsNullOrWhiteSpace(argsName))
                {
                    _builder.AppendFormat(" <{0}>", argsName);
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
                _builder.AppendFormat(" {0}", Synopsis.Options);
            }

            var argumentsName = commandDefinition.ArgumentDefinition?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                _builder.AppendFormat(" <{0}>", argumentsName);
            }

            if (hasCommand)
            {
                _builder.AppendFormat(" {0}", Synopsis.Command);
            }

            if (!commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                _builder.AppendFormat(" {0}", Synopsis.AdditionalArguments);
            }

            _builder.AppendLine();
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

            _builder.AppendLine();
            _builder.AppendLine(ArgumentsSection.Title);

            Indent();

            var maxWidth = showArgHelp ? GetHelpNameLength(argHelp) : 0;

            if (showParentArgHelp)
            {
                maxWidth = Math.Max(maxWidth, GetHelpNameLength(parentArgHelp));
            }

            maxWidth += ColumnGutterWidth;

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

            _builder.AppendLine();
            _builder.AppendLine(OptionsSection.Title);

            Indent();
            WriteOptionsList(options);
            Dedent();
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

            _builder.AppendLine();
            _builder.AppendLine(CommandsSection.Title);

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

            _builder.Append(AdditionalArgumentsSection.Title);
            _builder.AppendLine();
            Indent();
            _builder.Append(AdditionalArgumentsSection.Description);
            Dedent();
        }
    }
}
