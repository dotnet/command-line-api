using System.CommandLine;
using System.CommandLine.Invocation;
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
            };

            rootCommand.SetHandler((InvocationContext ctx, CancellationToken cancellationToken) =>
            {
                string apple = ctx.ParseResult.GetValue(appleOption);
                string banana = ctx.ParseResult.GetValue(bananaOption);
                string cherry = ctx.ParseResult.GetValue(cherryOption);
                string durian = ctx.ParseResult.GetValue(durianOption);

                return Task.FromResult(0);
            });

            var commandLine = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build();

            await commandLine.InvokeAsync(args);
        }
    }
}
