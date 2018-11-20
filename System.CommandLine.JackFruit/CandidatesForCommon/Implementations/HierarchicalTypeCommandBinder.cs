using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public class HierarchicalTypeCommandBinder<TRootType> : TypeCommandBinder
    {
        private readonly IEnumerable<IGrouping<Type, Type>> typesByBase;

        public HierarchicalTypeCommandBinder(
                    IDescriptionProvider<Type> descriptionProvider = null,
                    IHelpProvider<Type, PropertyInfo> helpProvider = null,
                    IOptionBinder<Type, PropertyInfo> optionProvider = null,
                    IArgumentBinder<Type, PropertyInfo> argumentProvider = null,
                    IInvocationProvider invocationProvider = null)
             : base(descriptionProvider, helpProvider, optionProvider, 
                   argumentProvider, invocationProvider)
        {
            typesByBase = typeof(TRootType).Assembly
                          .GetTypes()
                          .GroupBy(x => x.BaseType);
        }

        protected override IEnumerable<Type> GetSubCommandTypes(Type currentType)
        {
            return typesByBase
                    .Where(x => x.Key == currentType)
                    .SingleOrDefault();

        }

        // TODO: This is the wrong place for this method
        public static async Task<int> RunAsync(string[] args,
                   IDescriptionProvider<Type> descriptionProvider = null,
                   IInvocationProvider invocationProvider = null,
                   IRuleProvider ruleProvider = null)
        {
            var commandProvider = new HierarchicalTypeCommandBinder<TRootType>(
                        descriptionProvider, invocationProvider: invocationProvider);
            // TODO: Consider Get vs Create naming
            var command = commandProvider.GetRootCommand(typeof(TRootType));
            var builder = new CommandLineBuilder(command)
                .AddStandardDirectives()
                .UseExceptionHandler();

            var parser = builder.Build();
            return await parser.InvokeAsync(args);

        }
    }
}
