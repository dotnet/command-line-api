using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public static class Extensions
    {
        public static T AddCommands<T>(this T command, IEnumerable<Command> commands)
            where T : Command
        {
            if (commands == null)
            {
                return command;
            }
            foreach (var subCommand in commands)
            {
                command.AddCommand(subCommand);
            }
            return command;
        }


        public static Command AddOptions(this Command command, IEnumerable<Option> options)
        {
            if (options == null)
            {
                return command;
            }
            foreach (var option in options)
            {
                command.AddOption(option);
            }
            return command;
        }

        private static bool IsNameEqual(this string name1, string name2)
            => name1 == null || name2 == null
                ? name1 == name2
                : name1.ToKebabCase().ToLowerInvariant() == name2.ToKebabCase().ToLowerInvariant();

        public static object GetSymbolByName(this Command command, string name, bool lookHigher)
        {
            object symbol = null;
            if (command.Argument.Name.IsNameEqual(name))
            {
                return command.Argument;
            }
            if (command.Children.Contains(name))
            {
                return command.Children[name];
            }
            if (lookHigher && command.Parent != null)
            {
                return GetSymbolByName(command.Parent, name, lookHigher);
            }

            return symbol;
        }
    }
}
