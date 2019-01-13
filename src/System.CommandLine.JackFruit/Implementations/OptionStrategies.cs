using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class OptionStrategies 
    {
        public static (bool, IEnumerable<Option>) FromProperties(Command parent, Type baseType)
        {
            var propertyInfos = baseType.GetProperties();
            var filtered = propertyInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            var options = filtered.Select(p => GetOption(parent, p));
            return (false, options);
        }

        public static (bool, IEnumerable<Option>) FromParameters(Command parent, MethodInfo method)
        {
            var parameterInfos = method.GetParameters();
            var filtered = parameterInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            var options = filtered.Select(p => GetOption(parent, p));
            return (false, options);
        }

        public static Option GetOption(Command parent, object source)
        {
            var names = PreBinderContext.Current.AliasProvider.Get(parent, source)
                            .Select((x, n) => x.StartsWith("-")
                                              ? x
                                              : (n == 0 ? "--" : "-") + x);
            var arguments = PreBinderContext.Current.ArgumentProvider.Get(parent, source);
            var help = PreBinderContext.Current.DescriptionProvider.Get(parent, source);
            // TODO: Support IsHidden
            // TODO: Harvest default values from properties and parameters
            return new Option(new ReadOnlyCollection<string>(names.ToList()), help, arguments.FirstOrDefault());
        }
    }
}
