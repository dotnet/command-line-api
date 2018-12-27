using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public class PreBinder
    {
        public static Command RootCommand<TRootType>(IDescriptionFinder descriptionFinder)
        {
            PreBinderContext.Current.HelpFinder.AddApproach(
                HelpFinder.DescriptionFinderApproach(descriptionFinder));
            var command = CommandFinder.GetCommand(null, typeof(TRootType), new RootCommand());
            return command;
        }
    }
}
