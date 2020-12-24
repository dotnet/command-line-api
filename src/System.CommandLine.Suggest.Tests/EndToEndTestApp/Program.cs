using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace EndToEndTestApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>("--apple" ),
                new Option<string>("--banana"),
                new Option<string>("--cherry"),
                new Option<string>("--durian")
            };

            rootCommand.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(Run)));

            var commandLine = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build();

            await commandLine.InvokeAsync(args);
        }

        public static Task Run(string apple, string banana, string cherry, string durian) => Task.CompletedTask;
    }
}
