using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace DotMetal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parser = CreateCli.GetParserBuilder()
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
