using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace DotnetMetal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parser = CreateCli.GetParseBuilder()
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
