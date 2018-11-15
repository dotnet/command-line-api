using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public static class Runner
    {
        public static async Task<int> RunAsync<TResult>(string commandLine,
               IHelpProvider helpProvider = null,
               IInvocationProvider invocationProvider = null,
               IRuleProvider ruleProvider = null,
               AliasStyle aliasStyle = AliasStyle.Attribute,
               ArgumentStyle argumentStyle = ArgumentStyle.Attribute,
               PathStyle pathStyle = PathStyle.Path)
        {
            // TODO: Fix redundancy
            var builder = BuilderTools.CreateBuilder<TResult>(helpProvider, invocationProvider, ruleProvider,
                        aliasStyle, argumentStyle, pathStyle)
                .AddStandardDirectives()
                .UseExceptionHandler();
            // Create approach to add extra stuff
            Parser parser = builder.Build();
            return await parser.InvokeAsync(commandLine);

        }

        public static async Task<int> RunAsync<TResult>(string[] args,
              IHelpProvider helpProvider = null,
              IInvocationProvider invocationProvider = null,
              IRuleProvider ruleProvider = null,
              AliasStyle aliasStyle = AliasStyle.Attribute,
              ArgumentStyle argumentStyle = ArgumentStyle.Attribute,
              PathStyle pathStyle = PathStyle.Path)
        {
            var builder = BuilderTools.CreateBuilder<TResult>(helpProvider, invocationProvider, ruleProvider, 
                        aliasStyle, argumentStyle,pathStyle)
                .AddStandardDirectives()
                .UseExceptionHandler();
            // Create approach to add extra stuff
            Parser parser = builder.Build();
            return await parser.InvokeAsync(args);

        }
    }
}
