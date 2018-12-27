using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class ArgumentFinder : FinderBaseForList <Argument>
    {

        public ArgumentFinder(params Approach<IEnumerable<Argument>>[] approaches)
            : base(approaches: approaches)
        { }

        private static (bool, IEnumerable<Argument>) FromAttributedProperties(Command parent, Type baseType)
        {
            var properties = baseType.GetProperties();
            var attributedProperties = properties.Where(x => x.GetCustomAttribute<ArgumentAttribute>() !=null);
            return (false, baseType
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                    .Select(m => GetArgument(parent, m)));
        }

        private static (bool, IEnumerable<Argument>) FromSuffixedProperties(Command parent, Type baseType)
            => (false, baseType
                    .GetProperties()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parent, m)));

        private static (bool, IEnumerable<Argument>) FromAttributedParameters(Command parent, MethodInfo method)
            => (false, method
                 .GetParameters()
                 .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                 .Select(m => GetArgument(parent, m)));

        private static (bool, IEnumerable<Argument>) FromSuffixedParameters(Command parent, MethodInfo method)
            => (false, method
                    .GetParameters()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(parent,m)));

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        private static Argument GetArgument<T>(Command parent, T source)
        {
            var argument = new Argument();
            argument.Name = PreBinderContext.Current.AliasFinder.Get(parent, source).First();
            argument.Description = PreBinderContext.Current.HelpFinder.Get(parent, source);
            argument.ArgumentType = GetType(source);
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

        public static Approach<IEnumerable<Argument>> AttributedPropertyApproach() 
            => Approach<IEnumerable<Argument>>.CreateApproach<Type>(
                           (p,t) => FromAttributedProperties(p,t));
        public static Approach<IEnumerable<Argument>> SuffixedPropertyApproach()
             => Approach<IEnumerable<Argument>>.CreateApproach<Type>(
                           (p, t) => FromSuffixedProperties(p,t));

        public static Approach<IEnumerable<Argument>> AttributedParameterApproach()
             => Approach<IEnumerable<Argument>>.CreateApproach<MethodInfo>(
                           (m, t) => FromAttributedParameters(m,t));

        public static Approach<IEnumerable<Argument>> SuffixedParameterApproach()
             => Approach<IEnumerable<Argument>>.CreateApproach<MethodInfo>(
                           (m, t) => FromSuffixedParameters(m,t));

        public static ArgumentFinder Default() 
            => new ArgumentFinder(AttributedPropertyApproach(), SuffixedPropertyApproach(),
                AttributedParameterApproach(), SuffixedParameterApproach());
    }
}
