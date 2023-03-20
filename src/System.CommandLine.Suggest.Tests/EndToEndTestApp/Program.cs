using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using System.Threading;

namespace EndToEndTestApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var appleOption = new Option<string>("--apple" );
            var bananaOption = new Option<string>("--banana");
            var cherryOption = new Option<string>("--cherry");
            var durianOption = new Option<string>("--durian");

            var rootCommand = new RootCommand
            {
                appleOption,          
                bananaOption,          
                cherryOption,          
                durianOption,
                new HelpOption(),
                new VersionOption()
            };

            rootCommand.SetAction((ParseResult ctx, CancellationToken cancellationToken) =>
            {
                string apple = ctx.GetValue(appleOption);
                string banana = ctx.GetValue(bananaOption);
                string cherry = ctx.GetValue(cherryOption);
                string durian = ctx.GetValue(durianOption);

                return Task.CompletedTask;
            });

            CommandLineConfiguration commandLine = new (rootCommand);

            await commandLine.InvokeAsync(args);
        }
    }
}
