using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class OptionStrategies
    {
        public static  IEnumerable<SymbolBinding > FromProperties(Command parent, Type baseType)
        {
            var propertyInfos = baseType.GetProperties();
            var filtered = propertyInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            var bindingActions = filtered.Select(p => GetBindingAction(parent, p));
            return  bindingActions;
        }

        public static  IEnumerable<SymbolBinding> FromParameters(Command parent, MethodInfo method)
        {
            var parameterInfos = method.GetParameters();
            var filtered = parameterInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            var bindingActions = filtered.Select(p => GetBindingAction(parent, p));
            return  bindingActions;
        }

        private static SymbolBinding GetBindingAction(Command parent, ParameterInfo source) 
            => SymbolBinding.Create(source, GetOption(parent, source));

        private static SymbolBinding GetBindingAction(Command parent, PropertyInfo source)
            => SymbolBinding.Create(source, GetOption(parent, source));

        private static Option GetOption(Command parent, object source)
        {
            var names = PreBinderContext.Current.AliasProvider.Get(parent, source)
                            .Select((x, n) => x.StartsWith("-")
                                              ? x
                                              : (n == 0 ? "--" : "-") + x);
            var arguments = PreBinderContext.Current.ArgumentBindingProvider.Get(parent, source);
            var help = PreBinderContext.Current.DescriptionProvider.Get(parent, source);
            // TODO: Support IsHidden
            // TODO: Support default values
            var option = new Option(new ReadOnlyCollection<string>(names.ToList()), help, (Argument)arguments.FirstOrDefault().Symbol);
            return option;
        }
    }
}
