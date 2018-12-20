using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public class HierarchicalTypeCommandBinder<TRootType>
        : TypeCommandBinder<HierarchicalTypeCommandBinder<TRootType>>
    {
        private readonly IEnumerable<IGrouping<Type, Type>> typesByBase;

        // Generally you do not need to pass anything, except whether to remove parent names
        public HierarchicalTypeCommandBinder(
                    IDescriptionProvider<Type> descriptionProvider = null,
                    IHelpProvider<Type> helpProvider = null,
                    IOptionBinder<Type, PropertyInfo> optionProvider = null,
                    IArgumentBinder<Type, PropertyInfo> argumentProvider = null,
                    IInvocationProvider invocationProvider = null,
                    bool shouldRemoveParentNames = false)
             : base(descriptionProvider, helpProvider, optionProvider,
                   argumentProvider, invocationProvider, shouldRemoveParentNames)
            => typesByBase = typeof(TRootType).Assembly
                          .GetTypes()
                          .GroupBy(x => x.BaseType);

        public override IEnumerable<Type> GetSubCommandSources(Type currentType)
            => typesByBase
                    .Where(x => x.Key == currentType)
                    .SingleOrDefault();

        public static RootCommand GetRootCommand(
                   IDescriptionProvider<Type> descriptionProvider = null,
                   IInvocationProvider invocationProvider = null,
                   IRuleProvider ruleProvider = null,
                   bool shouldRemoveParentNames = true)
        {
            var commandProvider = new HierarchicalTypeCommandBinder<TRootType>(
                        descriptionProvider, invocationProvider: invocationProvider, 
                        shouldRemoveParentNames: shouldRemoveParentNames);
            // TODO: Consider Get vs Create naming
            return commandProvider.GetRootCommand(typeof(TRootType));
        }

        public static async Task<int> InvokeAsync(string[] args,
                   IDescriptionProvider<Type> descriptionProvider = null)
        {
            var command = GetRootCommand(descriptionProvider);
            return await command.InvokeAsync(args);
        }
    }
}
