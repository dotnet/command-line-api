using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace DotnetMetal
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new CommandLineBuilder()
                         // parser
                         .AddCommand(CommandDefinitions.Tool())
                         .AddVersionOption()

                         // middleware
                         .UseHelp()
                         .UseParseDirective()
                         .UseDebugDirective()
                         .UseSuggestDirective()
                         .RegisterWithDotnetSuggest()
                         .UseParseErrorReporting()
                         .UseExceptionHandler()

                         .Build();

            await parser.InvokeAsync(args);
        }
    }
}
