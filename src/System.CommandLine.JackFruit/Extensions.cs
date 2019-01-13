using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.JackFruit
{
    public static class Extensions
    {
        // Probably somewhere general
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }


        // I think these methods probably belong in CommandResult
        public static object GetValue(this ParseResult parseResult, Option option)
        {
            var result = parseResult.CommandResult.Children
                                   .Where(c => c.Symbol == option)
                                   .FirstOrDefault();
            switch (result)
            {
                case OptionResult optionResult:
                    return optionResult.GetValueOrDefault();
                case null:
                    var optionType = option.Argument.ArgumentType == null
                                    ? typeof(bool)
                                    : option.Argument.ArgumentType;
                    return optionType.GetDefaultValue();
                default:
                    throw new InvalidOperationException("Internal: Unknown result type");
            }
        }

        public static object GetValue(this ParseResult parseResult, Argument argument)
        {
            // TODO: Change when we support multiple arguments
            return parseResult.CommandResult.GetValueOrDefault();
        }

        public static object GetValue(this ParseResult parseResult, string name)
            => parseResult.CommandResult.Children
                  .Where(c => MatchName(c.Symbol.Name, name))
                  .FirstOrDefault();

        private static bool MatchName(string first, string second) 
            => first.ToKebabCase() == second.ToKebabCase();

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
            while (command != null)
            {
                if (command.Argument.Name.IsNameEqual(name))
                {
                    return command.Argument;
                }
                if (command.Children.Contains(name))
                {
                    return command.Children[name];
                }
                if (!lookHigher)
                {
                    break;
                }
                command = command.Parent;
            }

            return null;
        }
    }
}
