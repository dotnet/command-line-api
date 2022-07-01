using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

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

            rootCommand.SetHandler(
                (string apple, string banana, string cherry, string durian) => Task.CompletedTask,
                appleOption,
                bananaOption,
                cherryOption,
                durianOption);

            var commandLine = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build();

            await commandLine.InvokeAsync(args);
        }
    }
}
