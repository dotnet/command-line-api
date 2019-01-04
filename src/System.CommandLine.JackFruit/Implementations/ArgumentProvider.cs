using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public static class ArgumentStrategies
    {
        public static (bool, IEnumerable<Argument>) FromAttributedProperties(Command[] parents, Type baseType)
        {
            var properties = baseType.GetProperties();
            var attributedProperties = properties.Where(x => x.GetCustomAttribute<ArgumentAttribute>() != null);
            return (false, baseType
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                    .Select(m => GetArgument(parents, m)));
        }

        public static (bool, IEnumerable<Argument>) FromSuffixedProperties(Command[] parents, Type baseType)
            => (false, baseType
                    .GetProperties()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parents,  m)));

        public static (bool, IEnumerable<Argument>) FromAttributedParameters(Command[] parents, MethodInfo method)
            => (false, method
                 .GetParameters()
                 .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                 .Select(p => GetArgument(parents,  p)));

        public static (bool, IEnumerable<Argument>) FromSuffixedParameters(Command[] parents, MethodInfo method)
            => (false, method
                    .GetParameters()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parents,  m)));

        public static (bool, IEnumerable<Argument>) FromParameter(Command[] parents, ParameterInfo parameter)
            => (false, new List<Argument>() { GetArgument(parents,  parameter) });

        public static (bool, IEnumerable<Argument>) FromProperty(Command[] parents, PropertyInfo property)
            => (false, new List<Argument>() { GetArgument(parents,  property) });

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        private static Argument GetArgument<T>(Command[] parents,  T item)
        {
            var argument = new Argument();
            argument.Name = PreBinderContext.Current.AliasFinder.Get(parents, item).First();
            argument.Description = PreBinderContext.Current.DescriptionFinder.Get(parents, item);
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
