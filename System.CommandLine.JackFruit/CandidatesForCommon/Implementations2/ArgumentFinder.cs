using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class ArgumentFinder : FinderForListBase <Argument>
    {

        public ArgumentFinder(params Approach<IEnumerable<Argument>>[] approaches)
            : base(approaches: approaches)
        { }

        private static (bool, IEnumerable<Argument>) FromAttributedProperties(Type baseType)
            => (true, baseType
                        .GetProperties()
                        .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                        .Select(m => GetArgument(m)));

        private static (bool, IEnumerable<Argument>) FromSuffixedProperties(Type baseType)
            => (true, baseType
                    .GetProperties()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(m)));

        private static (bool, IEnumerable<Argument>) FromAttributedParameters(MethodInfo method)
            => (true, method
                 .GetParameters()
                 .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
                 .Select(m => GetArgument(m)));

        private static (bool, IEnumerable<Argument>) FromSuffixedParameters(MethodInfo method)
            => (true, method
                 .GetParameters()
                    .Where(p => NameIsSuffixed(p.Name))
                    .Select(m => GetArgument(m)));

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        private static Argument GetArgument<T>(T source)
        {
            var argument = new Argument();
            argument.Name = PreBinderContext.Current.AliasFinder.Get(source).First();
            argument.Description = PreBinderContext.Current.HelpFinder.Get(source);
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
                           t => FromAttributedProperties(t));
        public static Approach<IEnumerable<Argument>> SuffixedPropertyApproach()
             => Approach<IEnumerable<Argument>>.CreateApproach<Type>(
                            t => FromSuffixedProperties(t));

        public static Approach<IEnumerable<Argument>> AttributedParameterApproach()
             => Approach<IEnumerable<Argument>>.CreateApproach<MethodInfo>(
                            t => FromAttributedParameters(t));

        public static Approach<IEnumerable<Argument>> SuffixedParameterApproach()
             => Approach<IEnumerable<Argument>>.CreateApproach<MethodInfo>(
                            t => FromSuffixedParameters(t));

        public static ArgumentFinder Default() 
            => new ArgumentFinder(AttributedPropertyApproach(), SuffixedPropertyApproach(),
                AttributedParameterApproach(), SuffixedParameterApproach());
    }
}
