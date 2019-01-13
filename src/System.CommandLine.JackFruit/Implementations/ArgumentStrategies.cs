using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public static class ArgumentStrategies
    {
        public static (bool, IEnumerable<Argument>) FromAttributedProperties(Command parent, Type baseType)
        {
            var properties = baseType.GetProperties();
            var attributedProperties = properties.Where(x => x.GetCustomAttribute<ArgumentAttribute>() != null);
            return (false, baseType
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                    .Select(m => GetArgument(parent, m)));
        }

        public static (bool, IEnumerable<Argument>) FromSuffixedProperties(Command parent, Type baseType)
            => (false, baseType
                    .GetProperties()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parent,  m)));

        public static (bool, IEnumerable<Argument>) FromAttributedParameters(Command parent, MethodInfo method)
            => (false, method
                 .GetParameters()
                 .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                 .Select(p => GetArgument(parent,  p)));

        public static (bool, IEnumerable<Argument>) FromSuffixedParameters(Command parent, MethodInfo method)
            => (false, method
                    .GetParameters()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parent,  m)));

        public static (bool, IEnumerable<Argument>) FromParameter(Command parent, ParameterInfo parameter)
            => (false, new List<Argument>() { GetArgument(parent,  parameter) });

        public static (bool, IEnumerable<Argument>) FromProperty(Command parent, PropertyInfo property)
            => (false, new List<Argument>() { GetArgument(parent,  property) });

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

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
