using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class ArgumentFinder : FinderBaseForList<ArgumentFinder, Argument>
    {
        private static (bool, IEnumerable<Argument>) FromAttributedProperties(Command parent, Type baseType)
        {
            var properties = baseType.GetProperties();
            var attributedProperties = properties.Where(x => x.GetCustomAttribute<ArgumentAttribute>() != null);
            return (false, baseType
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                    .Select(m => GetArgument(parent, baseType, m)));
        }

        private static (bool, IEnumerable<Argument>) FromSuffixedProperties(Command parent, Type baseType)
            => (false, baseType
                    .GetProperties()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parent, baseType, m)));

        private static (bool, IEnumerable<Argument>) FromAttributedParameters(Command parent, MethodInfo method)
            => (false, method
                 .GetParameters()
                 .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                 .Select(p => GetArgument(parent, method, p)));

        private static (bool, IEnumerable<Argument>) FromSuffixedParameters(Command parent, MethodInfo method)
            => (false, method
                    .GetParameters()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parent, method, m)));

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        private static Argument GetArgument<T>(Command parent, object source, T item)
        {
            var argument = new Argument();
            argument.Name = PreBinderContext.Current.AliasFinder.Get(parent, item).First();
            argument.Description = PreBinderContext.Current.HelpFinder.Get(parent, source, item);
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

        public static ArgumentFinder Default()
            => new ArgumentFinder()
                    .AddApproachFromFunc<Type>(FromAttributedProperties)
                    .AddApproachFromFunc<Type>(FromSuffixedProperties)
                    .AddApproachFromFunc<MethodInfo>(FromAttributedParameters)
                    .AddApproachFromFunc<MethodInfo>(FromSuffixedParameters);
    }
}
