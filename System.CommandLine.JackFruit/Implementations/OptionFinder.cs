using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class OptionFinder : FinderBaseForList<OptionFinder, Option>
    {
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

        public static OptionFinder Default()
            => new OptionFinder()
                    .AddApproachFromFunc<Type>(FromProperties)
                    .AddApproachFromFunc<MethodInfo>(FromParameters);
    }
}
