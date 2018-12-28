using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class OptionFinder : FinderBaseForList<OptionFinder, Option>
    {
        // TODO: Do not add options for current items argument, or any parents arguments or options
        private static (bool, IEnumerable<Option>) FromProperties(Command[] parents, Type baseType)
        {
            var propertyInfos = baseType.GetProperties();
            var filtered = propertyInfos.Where(p => parents.GetSymbolByName(p.Name, true) == null);
            var options = filtered.Select(p => GetOption(parents, p));
            return (false, options);
        }

        private static (bool, IEnumerable<Option>) FromParameters(Command[] parents, MethodInfo method)
        {
            var parameterInfos = method.GetParameters();
            var filtered = parameterInfos.Where(p => parents.GetSymbolByName(p.Name, true) == null);
            var options = filtered.Select(p => GetOption(parents, p));
            return (false, options);
        }

        private static Option GetOption(Command[] parents, object source)
        {
            var names = PreBinderContext.Current.AliasFinder.Get(parents, source);
            // Argument Type must be property or parameter type
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(parents, source);
            var help = PreBinderContext.Current.HelpFinder.Get(parents, source);
            // TODO: Support IsHidden
            // TODO: Harvest default values from properties and parameters
            return new Option(new ReadOnlyCollection<string>(names.ToList()), help, arguments.FirstOrDefault());
        }

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        public static OptionFinder Default()
            => new OptionFinder()
                    .AddApproachFromFunc<Type>(FromProperties)
                    .AddApproachFromFunc<MethodInfo>(FromParameters);
    }
}
