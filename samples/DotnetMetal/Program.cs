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
                         .ConfigureParser()
                         .UseDebugDirective()
                         .UseParseErrorReporting()
                         .UseParseDirective()
                         .UseHelp()
                         .UseSuggestDirective()
                         .RegisterWithDotnetSuggest()
                         .UseExceptionHandler()
                         .Build();

            await parser.InvokeAsync(args);
        }
    }
}
