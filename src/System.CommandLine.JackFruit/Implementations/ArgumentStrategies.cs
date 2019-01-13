using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public static class ArgumentStrategies
    {
        public static IEnumerable<SymbolBinding> FromAttributedProperties(Command parent, Type baseType)
        {
            var properties = baseType.GetProperties();
            var attributedProperties = properties.Where(x => x.GetCustomAttribute<ArgumentAttribute>() != null);
            return baseType                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                    .Select(m => GetBindingAction(parent, m));
        }

        public static IEnumerable<SymbolBinding> FromSuffixedProperties(Command parent, Type baseType)
            => baseType                    .GetProperties()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetBindingAction(parent,  m));

        public static IEnumerable<SymbolBinding> FromAttributedParameters(Command parent, MethodInfo method)
            => method                 .GetParameters()
                 .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                 .Select(p => GetBindingAction(parent,  p));

        public static IEnumerable<SymbolBinding> FromSuffixedParameters(Command parent, MethodInfo method)
            => method                    .GetParameters()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetBindingAction(parent,  m));

        public static IEnumerable<SymbolBinding> FromParameter(Command parent, ParameterInfo parameter)
            =>  new List<SymbolBinding>() { GetBindingAction(parent,  parameter) };

        public static IEnumerable<SymbolBinding> FromProperty(Command parent, PropertyInfo property)
            =>  new List<SymbolBinding>() { GetBindingAction(parent,  property) };

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        private static SymbolBinding GetBindingAction(Command parent, ParameterInfo source)
           => SymbolBinding.Create(source, GetArgument(parent, source));

        private static SymbolBinding GetBindingAction(Command parent, PropertyInfo source)
            => SymbolBinding.Create(source, GetArgument(parent, source));

        private static Argument GetArgument<T>(Command parent,  T item)
        {
            var argument = new Argument();
            argument.Name = PreBinderContext.Current.AliasProvider.Get(parent, item).First();
            argument.Description = PreBinderContext.Current.DescriptionProvider.Get(parent, item);
            argument.ArgumentType = GetType(item);
            return argument;
        }

        private static Type GetType<T>(T source)
        {
            switch (source)
            {
                case PropertyInfo property:
                    return property.PropertyType;
                case ParameterInfo parameter:
                    return parameter.ParameterType;
                default:
                    return null;
            }
        }
    }
}
