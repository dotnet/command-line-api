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
            var rootCommand = new RootCommand
            {
                new Option("--apple", argument: new Argument<string>()),
                new Option("--banana", argument: new Argument<string>()),
                new Option("--cherry", argument: new Argument<string>()),
                new Option("--durian", argument: new Argument<string>())
            };

            rootCommand.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(Run)));

            var commandLine = new CommandLineBuilder(rootCommand)
                .UseHelp()
                .UseSuggestDirective()
                .UseExceptionHandler()
                .RegisterWithDotnetSuggest()
                .Build();

            await commandLine.InvokeAsync(args);
        }

        public static Task Run(string apple, string banana, string cherry, string durian) => Task.CompletedTask;
    }
}
