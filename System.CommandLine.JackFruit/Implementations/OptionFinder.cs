using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class OptionFinder : FinderBaseForList<Option>
    {
        public OptionFinder(params Approach<IEnumerable<Option>>[] approaches)
            : base (approaches: approaches )
        { }

        private static (bool, IEnumerable<Option>) FromProperties(Command parent, Type baseType)
            => (false, baseType
                 .GetProperties()
                 .Select(m => TypeBinder.BuildOption(m)));

        private static (bool, IEnumerable<Option>) FromParameters(Command parent, MethodInfo method)
            => (false, method
                 .GetParameters()
                 .Select(m => Invocation.Binder.BuildOption(m)));

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        public static Approach<IEnumerable<Option>> PropertyApproach()
            => Approach<IEnumerable<Option>>.CreateApproach<Type>(
                          (p, t) => FromProperties(p,t));

        public static Approach<IEnumerable<Option>> ParameterApproach()
             => Approach<IEnumerable<Option>>.CreateApproach<MethodInfo>(
                            (p, t) => FromParameters(p,t));


        public static OptionFinder Default()
            => new OptionFinder(PropertyApproach(), ParameterApproach());
    }
}
