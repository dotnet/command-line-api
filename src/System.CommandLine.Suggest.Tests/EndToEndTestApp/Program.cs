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
            CliOption<string> appleOption = new ("--apple" );
            CliOption<string> bananaOption = new ("--banana");
            CliOption<string> cherryOption = new ("--cherry");
            CliOption<string> durianOption = new ("--durian");

            CliRootCommand rootCommand = new ()
            {
                appleOption,          
                bananaOption,          
                cherryOption,          
                durianOption,
            };

            rootCommand.SetAction((ParseResult ctx, CancellationToken cancellationToken) =>
            {
                string apple = ctx.GetValue(appleOption);
                string banana = ctx.GetValue(bananaOption);
                string cherry = ctx.GetValue(cherryOption);
                string durian = ctx.GetValue(durianOption);

                return Task.CompletedTask;
            });

            CliConfiguration commandLine = new (rootCommand);

            await commandLine.InvokeAsync(args);
        }
    }
}
