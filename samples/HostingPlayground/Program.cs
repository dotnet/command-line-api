using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static HostingPlayground.HostingPlaygroundLogEvents;

namespace HostingPlayground
{
    class Program
    {
        static Task Main(string[] args) => BuildCommandLine()
            .UseHost(_ => Host.CreateDefaultBuilder(),
                host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddSingleton<IGreeter, Greeter>();
                    });
                })
            .InvokeAsync(args);

        private static CliConfiguration BuildCommandLine()
        {
            var root = new CliRootCommand(@"$ dotnet run --name 'Joe'"){
                new CliOption<string>("--name"){
                    Required = true
                }
            };
            root.Action = CommandHandler.Create<GreeterOptions, IHost>(Run);
            return new CliConfiguration(root);
        }

        private static void Run(GreeterOptions options, IHost host)
        {
            var serviceProvider = host.Services;
            var greeter = serviceProvider.GetRequiredService<IGreeter>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(Program));

            var name = options.Name;
            logger.LogInformation(GreetEvent, "Greeting was requested for: {name}", name);
            greeter.Greet(name);
        }
    }
}
