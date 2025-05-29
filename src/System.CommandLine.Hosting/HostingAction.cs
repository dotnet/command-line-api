using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.CommandLine.Invocation;

namespace System.CommandLine.Hosting;

public class HostingAction() : AsynchronousCommandLineAction()
{
    protected readonly Func<string[], IHostBuilder>? _createHostBuilder;
    internal Action<IHostBuilder>? ConfigureHost { get; set; }
    public Action<HostBuilderContext, IServiceCollection>? ConfigureServices { get; set; }

    protected virtual IHostBuilder CreateHostBuiderCore(string[] args)
    {
        var hostBuilder = _createHostBuilder?.Invoke(args) ??
            new HostBuilder();
        return hostBuilder;
    }
    
    protected virtual void ConfigureHostBuilder(IHostBuilder hostBuilder)
    {
        ConfigureHost?.Invoke(hostBuilder);
        if (ConfigureServices is not null)
        {
            hostBuilder.ConfigureServices(ConfigureServices);
        }
    }

    public override async Task<int> InvokeAsync(
        ParseResult parseResult, 
        CancellationToken cancellationToken = default
        )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(parseResult);
#else
        _ = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
#endif

        string[] unmatchedTokens = parseResult.UnmatchedTokens?.ToArray() ?? [];
        IHostBuilder hostBuilder = CreateHostBuiderCore(unmatchedTokens);
        hostBuilder.Properties[typeof(ParseResult)] = parseResult;

        // As long as done before first await
        // ProcessTerminationTimeout can be set to null
        // so that .NET Generic Host can control console lifetime instead.
        parseResult.Configuration.ProcessTerminationTimeout = null;
        hostBuilder.UseConsoleLifetime();

        hostBuilder.ConfigureServices(static (context, services) =>
        {
            var parseResult = context.GetParseResult();
            var hostingAction = parseResult.GetHostingAction();
            services.AddSingleton(parseResult);
            services.AddSingleton(parseResult.Configuration);
            services.AddHostedService<HostingActionService>();
            // TODO: add IHostingActionInvocation singleton
        });

        ConfigureHostBuilder(hostBuilder);

        using var host = hostBuilder.Build();
        await host.StartAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        var appRunningTask = host.WaitForShutdownAsync(cancellationToken);

        // TODO: Retrieve ExecuteTask from HostingActionService to get result
        Task<int> invocationTask = Task.FromResult(0);

        await appRunningTask.ConfigureAwait(continueOnCapturedContext: false);

        return await invocationTask
            .ConfigureAwait(continueOnCapturedContext: false);
    }
}