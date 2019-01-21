using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class OptionProvider
    {
        public static IEnumerable<(object Source, Option Option)> FromProperties(Command parent, Type baseType)
        {
            var propertyInfos = baseType.GetProperties();
            // Do arguents first and assume that if it is already attached, its an argument
            var filtered = propertyInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            return filtered.Select(p => ((object)p, GetOption(parent, p)));
        }

        public static IEnumerable<(object Source, Option Option)> FromParameters(Command parent, MethodInfo method)
        {
            var parameterInfos = method.GetParameters();
            // Do arguents first and assume that if it is already attached, its an argument
            var filtered = parameterInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            return filtered.Select(p => ((object)p, GetOption(parent, p)));
        }

        private static Option GetOption(Command parent, object source)
        {
            var names = PreBinderContext.Current.AliasStrategies.Get(parent, source)
                            .Select((x, n) => x.StartsWith("-")
                                              ? x
                                              : (n == 0 ? "--" : "-") + x);
            var arguments = PreBinderContext.Current.ArgumentBindingStrategies.Get(parent, source);
            var help = PreBinderContext.Current.DescriptionStrategies.Get(parent, source);
            // TODO: Support IsHidden
            // TODO: Support default values
            var option = new Option(new ReadOnlyCollection<string>(names.ToList()), help, arguments.FirstOrDefault().Argument );
            return option;
        }
    }
}
