using System;
using System.Linq;
using System.Text;

namespace CommandLine
{
    public static class ParserExtensions
    {
        public static string HelpView(this Parser parser) =>
            parser.DefinedOptions
                  .FlattenBreadthFirst()
                  .OfType<Command>()
                  .FirstOrDefault()
                  ?.HelpView() ??
            parser.DefinedOptions
                  .First()
                  .HelpText;

        public static string HelpView(this Command command)
        {
            var options = command.DefinedOptions.ToArray();

            var commands = command.AllCommands().ToArray();

            options = options
                .Where(o => !o.IsCommand)
                .ToArray();

            var s = new StringBuilder();

            s.Append("usage: ");

            s.Append(command.FullyQualifiedName());

            if (commands.Any())
            {
                s.Append(" [<options>]");
            }

            s.AppendLine(" [--] <operands>...");

            s.AppendLine();

            foreach (var option in options.Where(o => !o.IsHidden()))
            {
                WriteHelpText(option, s);
            }

            return s.ToString();
        }

        public static string HelpView(this Option option)
        {
            var command = option as Command;
            if (command != null)
            {
                return command.HelpView();
            }

            var s = new StringBuilder();

            WriteHelpText(option, s);

            return s.ToString();
        }

        private static void WriteHelpText(Option option, StringBuilder s)
        {
            var optionString = "    " +
                               string.Join(", ",
                                           option.Aliases
                                                 .OrderBy(a => a.Length)
                                                 .Select(a => a.Length == 1
                                                                  ? $"-{a}"
                                                                  : $"--{a}"));

            s.Append(optionString);

            var colWidth = 26;

            if (optionString.Length <= colWidth - 2)
            {
                s.Append(new string(' ', colWidth - optionString.Length));
            }
            else
            {
                s.AppendLine();
                s.Append(new string(' ', colWidth));
            }

            s.AppendLine(option.HelpText);
        }

        public static ParseResult Parse(this Parser parser, string s) =>
            parser.Parse(s.Tokenize().ToArray(),
                         isProgressive: !s.EndsWith(" "));
    }
}