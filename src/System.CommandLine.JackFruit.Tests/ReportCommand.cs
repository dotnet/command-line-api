using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public static class Reporter
    {
        private static int indentSpaces = 3;

        public static string ReportCommand(Command command, int indent = 0)
            => "\n" + new string(' ', indent * indentSpaces) + "Command:" +
                $"{(command is RootCommand ? typeof(RootCommand).Name : command.Name)} " +
                $"Help: {command.Description}" +
                string.Join("", ReportArgument(command.Argument, indent + 1)) +
                string.Join("", command.Children.OfType<Option>().Select(x => ReportOption(x, indent + 1))) +
                string.Join("", command.Children.OfType<Command>().Select(x => ReportCommand(x, indent + 1)));

        public static string ReportOption(Option option, int indent = 0) 
            => "\n" +new string(' ', indent * indentSpaces) + "Option:" +
                $"{option.Name} Type: {option.Argument.ArgumentType} Help: {option.Description}";

        public static string ReportArgument(Argument argument, int indent)
            => "\n" + new string(' ', indent * indentSpaces) + "Argument:" +
                $"{argument.Name} Type: {argument.ArgumentType} Help: {argument.Description}";

    }
}
