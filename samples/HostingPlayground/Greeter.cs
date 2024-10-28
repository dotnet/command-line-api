using System;
using System.CommandLine.Hosting;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static HostingPlayground.HostingPlaygroundLogEvents;

namespace HostingPlayground;

public class Greeter : CliHostedService
{
    private readonly GreeterOptions options;
    private readonly ILogger<Greeter> logger;

    public Greeter(
        IOptions<GreeterOptions> options,
        ILogger<Greeter> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    protected override Task<int> InvokeAsync(CancellationToken stoppingToken)
    {
        string name = options.Name;
        logger.LogInformation(GreetEvent, "Greeting was requested for: {name}", name);
        Greet(name);
        
        return Task.FromResult(0);
    }

    private static void Greet(string name) => Console.WriteLine($"Hello, {name ?? "anonymous"}");
}
