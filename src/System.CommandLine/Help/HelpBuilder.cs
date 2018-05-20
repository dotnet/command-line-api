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
        private int _indentationLevel;
        private int _currentIndentation;
        private const int IndentationSize = 2;
        public int MaxWidth { get; private set; }

        private const int DefaultWindowWidth = 80;
        private const int MaxWidthLeeWay = 2;
        private const int ColumnGutterWidth = 3;

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
            return MaxWidth - _currentIndentation - MaxWidthLeeWay - usedWidth;
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

        public string Build(CommandDefinition commandDefinition, int? maxWidth = null)
        {
            if (commandDefinition == null)
            {
                throw new ArgumentNullException(nameof(commandDefinition));
            }

            MaxWidth = maxWidth ?? GetWindowWidth();

            WriteSynopsis(commandDefinition);
            WriteArgumentsSection(commandDefinition);
            WriteOptionsSection(commandDefinition);
            WriteSubcommandsSection(commandDefinition);
            WriteAdditionalArgumentsSection(commandDefinition);
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

        private void WriteSynopsis(CommandDefinition commandDefinition)
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

            var hasHelp = commandDefinition.SymbolDefinitions
                .Where(symbolDef => !(symbolDef is CommandDefinition))
                .Any(symbolDef => symbolDef.HasHelp);

            if (hasHelp)
            {
                _builder.AppendFormat(" {0}", Synopsis.Options);
            }

            var argumentsName = commandDefinition.ArgumentDefinition?.Help?.Name;
            if (!string.IsNullOrWhiteSpace(argumentsName))
            {
                _builder.AppendFormat(" <{0}>", argumentsName);
            }

            if (commandDefinition.SymbolDefinitions.OfType<CommandDefinition>().Any())
            {
                _builder.AppendFormat(" {0}", Synopsis.Command);
            }

            if (!commandDefinition.TreatUnmatchedTokensAsErrors)
            {
                _builder.AppendFormat(" {0}", Synopsis.AdditionalArguments);
            }

            _builder.AppendLine();
        }

        private void WriteArgumentsSection(CommandDefinition commandDefinition)
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

        private void WriteOptionsSection(CommandDefinition commandDefinition)
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

        private void WriteSubcommandsSection(CommandDefinition commandDefinition)
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

        private void WriteAdditionalArgumentsSection(CommandDefinition commandDefinition)
        {
            if (commandDefinition?.TreatUnmatchedTokensAsErrors == true)
            {
                return;
            }

            _builder.Append(AdditionalArgumentsSection);
        }
    }
}
