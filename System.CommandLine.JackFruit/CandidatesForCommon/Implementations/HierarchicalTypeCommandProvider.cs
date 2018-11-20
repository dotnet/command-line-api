using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public class HierarchicalTypeCommandProvider<TRootType> : TypeCommandProvider
    {
        private readonly IEnumerable<IGrouping<Type, Type>> typesByBase;

        public HierarchicalTypeCommandProvider(
                    IDescriptionProvider<Type> descriptionProvider = null,
                    IHelpProvider<Type, PropertyInfo> helpProvider = null,
                    IOptionProvider<Type, PropertyInfo> optionProvider = null,
                    IArgumentProvider<Type, PropertyInfo> argumentProvider = null,
                    IInvocationProvider invocationProvider = null)
             : base(descriptionProvider, helpProvider, optionProvider, argumentProvider, invocationProvider)
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

        public static async Task<int> RunAsync(string[] args,
                   IDescriptionProvider<Type> descriptionProvider = null,
                   IInvocationProvider invocationProvider = null,
                   IRuleProvider ruleProvider = null)
        {
            var commandProvider = new HierarchicalTypeCommandProvider<TRootType>(
                        descriptionProvider, invocationProvider: invocationProvider);
            var command = commandProvider.GetRootCommand(typeof(TRootType));
            var builder = new CommandLineBuilder(command)
                .AddStandardDirectives()
                .UseExceptionHandler();

            var parser = builder.Build();
            return await parser.InvokeAsync(args);

        }
    }
}
