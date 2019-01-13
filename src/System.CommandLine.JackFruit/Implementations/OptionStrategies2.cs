using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class OptionStrategies2
    {
        public static (bool, IEnumerable<SymbolBindingAction >) FromProperties(Command parent, Type baseType)
        {
            var propertyInfos = baseType.GetProperties();
            var filtered = propertyInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            var bindingActions = filtered.Select(p => GetBindingAction(parent, p));
            return (false, bindingActions);
        }

        public static (bool, IEnumerable<SymbolBindingAction>) FromParameters(Command parent, MethodInfo method)
        {
            var parameterInfos = method.GetParameters();
            var filtered = parameterInfos.Where(p => parent.GetSymbolByName(p.Name, true) == null);
            var bindingActions = filtered.Select(p => GetBindingAction(parent, p));
            return (false, bindingActions);
        }

        public static SymbolBindingAction GetBindingAction(Command parent, ParameterInfo source) 
            => SymbolBindingAction.Create(source, GetOption(parent, source));

        public static SymbolBindingAction GetBindingAction(Command parent, PropertyInfo source)
            => SymbolBindingAction.Create(source, GetOption(parent, source));

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
            var option = new Option(new ReadOnlyCollection<string>(names.ToList()), help, arguments.FirstOrDefault());
            return option;
        }
    }
}
