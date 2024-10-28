#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostedServiceAction<THostedService, THostBuilder> : HostingAction<THostBuilder>
    where THostedService : CliHostedService
{
    internal HostedServiceAction(
        Func<string[], THostBuilder> hostBuilderFactory,
        Action<THostBuilder>? configureHost = null,
        Func<THostBuilder, IHostBuilder>? builderAsHostBuilder = null
        ) : base(
            hostBuilderFactory,
            configureHost,
            builderAsHostBuilder
            )
    { }

    protected override async Task<int> InvokeHostAsync(
        IHost host,
        CancellationToken cancellationToken
        )
    {
        var cancelSource = new TaskCompletionSource<int>();
        using var cancelRegistration = cancellationToken.Register(
            static state => ((TaskCompletionSource<int>)state!).SetCanceled(),
            cancelSource
            );
        var execTasks = host.Services
            .GetServices<IHostedService>()
            .OfType<THostedService>()
            .Select(service => service.ExecuteTask!)
            ;
        var resultTask = await Task.WhenAny(
            WaitForCompletion(execTasks),
            cancelSource.Task
            ).ConfigureAwait(continueOnCapturedContext: false);
        return await resultTask.ConfigureAwait(continueOnCapturedContext: false);

        static async Task<int> WaitForCompletion(IEnumerable<Task<int>> tasks)
        {
            var results = await Task.WhenAll(tasks)
                .ConfigureAwait(continueOnCapturedContext: false);
            return results.FirstOrDefault(exitCode => exitCode != default);
        }
    }
}

public static class HostedServiceAction
{
    public static HostedServiceAction<THostedService, IHostBuilder> Create<THostedService>(
        Func<string[], IHostBuilder>? hostBuilderFactory,
        Action<IHostBuilder>? configureHost = null
        )
        where THostedService : CliHostedService
        => new(
            hostBuilderFactory ?? Host.CreateDefaultBuilder,
            configureHost + ConfigureHostDefaults<THostedService>
            );

    public static HostedServiceAction<THostedService, IHostBuilder> Create<THostedService>(
        Action<IHostBuilder>? configureHost = null
        )
        where THostedService : CliHostedService
        => Create<THostedService>(hostBuilderFactory: default, configureHost);

    private static void ConfigureHostDefaults<THostedService>(IHostBuilder hostBuilder)
        where THostedService : CliHostedService
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.AddHostedService<THostedService>();
        });
    }

#if NET8_0_OR_GREATER
    public static HostedServiceAction<THostedService, HostApplicationBuilder> Create<THostedService>(
        Func<string[], HostApplicationBuilder>? hostBuilderFactory,
        Action<HostApplicationBuilder>? configureHost = null
        )
        where THostedService : CliHostedService
        => new(
            hostBuilderFactory ?? Host.CreateApplicationBuilder,
            configureHost + ConfigureHostDefaults<THostedService>,
            static builder => new HostApplicationBuilderAsIHostBuilder(builder)
            );

    public static HostedServiceAction<THostedService, HostApplicationBuilder> Create<THostedService>(
        Action<HostApplicationBuilder>? configureHost = null
        )
        where THostedService : CliHostedService
        => Create<THostedService>(hostBuilderFactory: default, configureHost);

    private static void ConfigureHostDefaults<THostedService>(HostApplicationBuilder hostAppBuilder)
        where THostedService : CliHostedService
    {
        hostAppBuilder.Services.AddHostedService<THostedService>();
    }
#endif
}