#nullable enable

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public abstract class CliHostedService : BackgroundService
{
    /// <inheritdoc cref="BackgroundService.ExecuteTask" />
    public new Task<int>? ExecuteTask => base.ExecuteTask as Task<int>;

    protected sealed override Task ExecuteAsync(CancellationToken stoppingToken) =>
        InvokeAsync(stoppingToken);
    
    protected abstract Task<int> InvokeAsync(CancellationToken cancelToken);
}
