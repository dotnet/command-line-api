using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class OptionStrategies 
    {
        // TODO: Do not add options for current items argument, or any parents arguments or options
        public static (bool, IEnumerable<Option>) FromProperties(Command[] parents, Type baseType)
        {
            var propertyInfos = baseType.GetProperties();
            var filtered = propertyInfos.Where(p => parents.GetSymbolByName(p.Name, true) == null);
            var options = filtered.Select(p => GetOption(parents, p));
            return (false, options);
        }

        public static (bool, IEnumerable<Option>) FromParameters(Command[] parents, MethodInfo method)
        {
            var parameterInfos = method.GetParameters();
            var filtered = parameterInfos.Where(p => parents.GetSymbolByName(p.Name, true) == null);
            var options = filtered.Select(p => GetOption(parents, p));
            return (false, options);
        }

        public static Option GetOption(Command[] parents, object source)
        {
            var names = PreBinderContext.Current.AliasProvider.Get(parents, source)
                            .Select((x, n) => x.StartsWith("-")
                                              ? x
                                              : (n == 0 ? "--" : "-") + x);
            var arguments = PreBinderContext.Current.ArgumentProvider.Get(parents, source);
            var help = PreBinderContext.Current.DescriptionProvider.Get(parents, source);
            // TODO: Support IsHidden
            // TODO: Harvest default values from properties and parameters
            return new Option(new ReadOnlyCollection<string>(names.ToList()), help, arguments.FirstOrDefault());
        }
    }
}
