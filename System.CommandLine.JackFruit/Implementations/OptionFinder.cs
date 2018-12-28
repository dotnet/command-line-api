using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class OptionFinder : FinderBaseForList<OptionFinder, Option>
    {
        // TODO: Do not add options for current items argument, or any parent arguments or options
        private static (bool, IEnumerable<Option>) FromProperties(Command parent, Type baseType)
            => (false, baseType
                 .GetProperties()
                 .Where(p=>!parent.Children.Contains(p.Name))
                 .Select(p => GetOption(parent, p)));

        private static (bool, IEnumerable<Option>) FromParameters(Command parent, MethodInfo method)
            => (false, method
                 .GetParameters()
                 .Where(p => !parent.Children.Contains(p.Name))
                 .Select(p => GetOption(parent, p)));

        private static Option GetOption(Command parent, object source)
        {
            var names = PreBinderContext.Current.AliasFinder.Get(parent, source);
            // Argument Type must be property or parameter type
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(parent, source);
            var help = PreBinderContext.Current.HelpFinder.Get(parent, source);
            // TODO: Support IsHidden
            // TODO: Harvest default values from properties and parameters
            return new Option(new ReadOnlyCollection<string>(names.ToList()), help, arguments.FirstOrDefault());
        }

        // TODO: Also need to update the name to remove Args, and this is a can of worms
        private static bool NameIsSuffixed(string name)
            => name.EndsWith("Args");

        public static OptionFinder Default()
            => new OptionFinder()
                    .AddApproachFromFunc<Type>(FromProperties)
                    .AddApproachFromFunc<MethodInfo>(FromParameters);
    }
}
