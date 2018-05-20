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
        private readonly StringBuilder _builder;
        private readonly CommandDefinition _commandDefinition;
        private int _indentationLevel;
        private int _currentIndentation;
        private const int IndentationSize = 2;
        private readonly int _windowWidth;

        private const int DefaultWindowWidth = 80;
        private const int MaxWidthLeeWay = 2;
        private const int ColumnGutterWidth = 3;

        public HelpBuilder(CommandDefinition commandDefinition, int? windowWidth = null)
        {
            if (commandDefinition == null)
            {
                throw new ArgumentNullException(nameof(commandDefinition));
            }

            _builder = new StringBuilder();
            _commandDefinition = commandDefinition;
            _windowWidth = windowWidth ?? GetWindowWidth();
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

        internal void AddHeader(string header)
        {
            AddIndentedText(header);
            _builder.AppendLine();
        }

        internal void AddIndentedText(string text)
        {
            _builder.Append(new string(' ', _currentIndentation));
            _builder.Append(text);
        }

        internal void AddLine(string name, int padding, string description)
        {
            _builder.AppendFormat(
                "{0}{1}{2}{3}{4}",
                new string(' ', _currentIndentation),
                name,
                new string(' ', padding),
                description,
                NewLine);
        }

        internal int GetAvailableWidth(int usedWidth = 0)
        {
            return _windowWidth - _currentIndentation - MaxWidthLeeWay - usedWidth;
        }

        internal void AddPadding(int width)
        {
            _builder.Append(new string(' ', width));
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

        private static int GetTextLength(HelpDefinition help)
        {
            return help?.Name?.Length ?? 0;
        }

        public string Build()
        {
            WriteSynopsis();
            WriteArgumentsSection();
            WriteOptionsSection();
            WriteSubcommandsSection();
            WriteAdditionalArgumentsSection();
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
            var builder = new StringBuilder();
            builder.Append(IndentationSize);
            builder.Append(string.Join(", ", symbolDefinition.RawAliases.OrderBy(alias => alias.Length)));

            var argumentName = symbolDefinition.ArgumentDefinition?.Help?.Name;

            if (!string.IsNullOrWhiteSpace(argumentName))
            {
                builder.Append($" <{argumentName}>");
            }

            return builder.ToString();
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

        private void WriteSynopsis()
        {
            _builder.Append(Synopsis.Title);

            _builder.AppendFormat(" {0}", _commandDefinition.Name);

            var subcommands = _commandDefinition
                .RecurseWhileNotNull(commandDef => commandDef.Parent)
                .Where(commandDef => commandDef != _commandDefinition)
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

            var hasHelp = _commandDefinition.SymbolDefinitions
                .Where(symbolDef => !(symbolDef is CommandDefinition))
                .Any(symbolDef => symbolDef.HasHelp);

            if (hasHelp)
            {
                _builder.AppendFormat(" {0}", Synopsis.Options);
            }

            var argumentsName = _commandDefinition.ArgumentDefinition?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                _builder.AppendFormat(" <{0}>", argumentsName);
            }

            if (_commandDefinition.SymbolDefinitions.OfType<CommandDefinition>().Any())
            {
                _builder.AppendFormat(" {0}", Synopsis.Command);
            }

            if (!_commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                _builder.AppendFormat(" {0}", Synopsis.AdditionalArguments);
            }

            _builder.AppendLine();
        }

        private void WriteArgumentsSection()
        {
            var showArgHelp = _commandDefinition.HasArguments && _commandDefinition.HasHelp;
            var showParentArgHelp = false;

            if (_commandDefinition.Parent != null)
            {
                showParentArgHelp = _commandDefinition.Parent.HasArguments && _commandDefinition.Parent.HasHelp;
            }

            if (!showArgHelp && !showParentArgHelp)
            {
                return;
            }

            var argHelp = _commandDefinition.ArgumentDefinition?.Help;
            var parentArgHelp = _commandDefinition.Parent?.ArgumentDefinition?.Help;

            _builder.AppendLine();
            _builder.AppendLine(ArgumentsSection.Title);

            Indent();

            var maxWidth = showArgHelp ? GetTextLength(argHelp) : 0;

            if (showParentArgHelp)
            {
                maxWidth = Math.Max(maxWidth, GetTextLength(parentArgHelp));
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

        private void WriteOptionsSection()
        {
            var options = _commandDefinition
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

        private void WriteSubcommandsSection()
        {
            var subcommands = _commandDefinition
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

        private void WriteAdditionalArgumentsSection()
        {
            if (_commandDefinition?.TreatUnmatchedTokensAsErrors == true)
            {
                return;
            }

            _builder.Append(AdditionalArgumentsSection);
        }
    }
}
