using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EndToEndTestApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var commandLine = new CommandLineBuilder()
                .UseHelp()   
                .UseSuggestDirective()
                .UseExceptionHandler()
                .RegisterWithDotnetSuggest()
                .ConfigureFromMethod(typeof(Program).GetMethod(nameof(Run)))
                .Build();

            await commandLine.InvokeAsync(args);
        }

        public static Task Run(string apple, string banana, string cherry, string durian) => Task.CompletedTask;
    }
}
