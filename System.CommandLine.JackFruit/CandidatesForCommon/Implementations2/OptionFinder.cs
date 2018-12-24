using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class OptionFinder : FinderForListBase<Option>
    {
        public OptionFinder(params Approach<IEnumerable<Option>>[] approaches)
            : base (approaches: approaches )
        { }

        private static (bool, IEnumerable<Option>) FromProperties(Type baseType)
            => (true, baseType
                 .GetProperties()
                 .Select(m => TypeBinder.BuildOption(m)));

        private static (bool, IEnumerable<Option>) FromParameters(MethodInfo method)
            => (true, method
                 .GetParameters()
                 .Select(m => Invocation.Binder.BuildOption(m)));

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

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

        public static Approach<IEnumerable<Option>> PropertyApproach()
            => Approach<IEnumerable<Option>>.CreateApproach<Type>(
                           t => FromProperties(t));

        public static Approach<IEnumerable<Option>> ParameterApproach()
             => Approach<IEnumerable<Option>>.CreateApproach<Type>(
                            t => FromProperties(t));


        public static OptionFinder Default()
            => new OptionFinder(PropertyApproach(), ParameterApproach());
    }
}
