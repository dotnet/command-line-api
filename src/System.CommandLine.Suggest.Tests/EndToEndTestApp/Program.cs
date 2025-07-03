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
            Option<string> appleOption = new ("--apple" );
            Option<string> bananaOption = new ("--banana");
            Option<string> cherryOption = new ("--cherry");
            Option<string> durianOption = new ("--durian");

            RootCommand rootCommand = new ()
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

            await rootCommand.Parse(args).InvokeAsync();
        }
    }
}
