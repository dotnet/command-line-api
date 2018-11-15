using System.CommandLine.JackFruit;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System;
using System.CommandLine;

namespace JackFruit
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // With all defaults - this won't work correctly because this result is
            // altered to show features. 
            var commandLine = "jackfruit tool install foo".Split(" ");
            var retValue = await Runner.RunAsync<DotnetJackFruit>(commandLine);

            // With features shown
            var tool = BuilderTools.CreateBuilder<Tool>(new HelpProvider());
            var sln = BuilderTools.CreateBuilder<Sln>(invocationProvider: new InvocationProvider());
            var add = BuilderTools.CreateBuilder<Add>(new HelpProvider(), aliasStyle: AliasStyle.Underscore);
            var list = BuilderTools.CreateBuilder<List>(new HelpProvider(), argumentStyle: ArgumentStyle.ConstructorArg );
            var remove = BuilderTools.CreateBuilder<Remove>(new HelpProvider(), pathStyle: PathStyle.Simple);
            var nuget = BuilderTools.CreateBuilder<NuGet>(new HelpProvider(), ruleProvider: new RuleProvider());

            Console.Read();
            return retValue;

            // Alternatives
            // Help: Provider or attributes. Attributes when no provider given
            // Invocation: Provider or Invoke method on class: Invoke method required when no provider given
            // RuleProvider: Provider or attributes. Attributes when no provider given
            // Alias: Attribute or underscore: Attribute default. Consider adding provider if appears to have value
            // ArgumentStyle: Attribute or ConstructorArg: Attribute default. Consider a naming pattern.
            // PathStyle: Simple, Path, AbsoluteSimple: Simple is default. This crates the ID providers need
            // 
            // There are two related provlems with ambiguity: how paths are defined and how class names avoid collision. 
            // Currently namespaces are ignored in path creation, so namespaces can be used to solve collisions.
            // If any providers are used, they need unique Ids. Path 
            // - PathStyle concatenates ancestors with a slash. Class ambinguity can then be solved with nesting or namespaces.
            // - Simple expects unique class names and strips ancestors from class names when creating command names.
            // - AbosluteSimple does neither. 
        }
    }

    internal class RuleProvider : IRuleProvider
    {
    }

    internal class NuGet
    {
    }

    internal class Remove : DotnetJackFruit
    {
    }

    internal class List : DotnetJackFruit
    {
    }

    internal class Add : DotnetJackFruit
    {
    }

    internal class InvocationProvider : IInvocationProvider
    {
        public Func<Task<int>> InvokeAsyncFunc<T>(T command) where T : Command => throw new NotImplementedException();
    }
}
