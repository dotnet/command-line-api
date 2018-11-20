using System.CommandLine.JackFruit;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.CommandLine;

namespace JackFruit
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var commandLine = new string[] { "tool", "-h" };
            var retValue = await HierarchicalTypeCommandBinder<DotnetJackFruit>.RunAsync(
                        commandLine, new DescriptionProvider(), new InvocationProvider());

            return retValue;

            // Notes - ignore
            // Help: Provider or attributes. If an attribute exists, provider is not called
            // Invocation: Provider or Invoke method on class: Invoke method required when no provider given or provider returns null
            // RuleProvider: Provider or attributes. If both are present, the results are merged
            // Alias: Attribute or underscore: If an attribute exists, it is used and underscores are ignored
            // ArgumentStyle: Attribute, ConstructorArg or ends with Arg: If both exist,attribute wins
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
}
