using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public static class ArgumentProvider
    {
        public static IEnumerable<(object Source, Argument Argument)> FromAttributedProperties(Command parent, Type baseType)
        {
            var properties = baseType.GetProperties();
            var attributedProperties = properties.Where(x => x.GetCustomAttribute<ArgumentAttribute>() != null);
            return baseType.GetProperties()
                    .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                    .Select(m => ((object) m, GetArgument(parent, m)));
        }

        public static IEnumerable<(object Source, Argument Argument)> FromSuffixedProperties(Command parent, Type baseType)
            => baseType.GetProperties()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => ((object)m, GetArgument(parent, m)));

        public static IEnumerable<(object Source, Argument Argument)> FromAttributedParameters(Command parent, MethodInfo method)
            => method.GetParameters()
                    .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                    .Select(m => ((object)m, GetArgument(parent, m)));

        public static IEnumerable<(object Source, Argument Argument)> FromSuffixedParameters(Command parent, MethodInfo method)
            => method.GetParameters()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => ((object)m, GetArgument(parent, m)));

        public static IEnumerable<(object Source, Argument Argument)> FromParameter(Command parent, ParameterInfo parameter)
            => new List<(object Source, Argument Argument)>() { ((object)parameter , GetArgument(parent, parameter)) };

        public static IEnumerable<(object Source, Argument Argument)> FromProperty(Command parent, PropertyInfo property)
            => new List<(object Source, Argument Argument)>() { ((object)property, GetArgument(parent, property)) };

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        private static Argument GetArgument<T>(Command parent, T item)
        {
            var argument = new Argument();
            argument.Name = PreBinderContext.Current.AliasStrategies.Get(parent, item).First();
            argument.Description = PreBinderContext.Current.DescriptionStrategies.Get(parent, item);
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
