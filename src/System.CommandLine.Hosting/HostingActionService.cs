using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

internal class HostingActionService(
    IHostApplicationLifetime lifetime,
    IHostingActionInvocation invocation
    ) : BackgroundService()
{
    public new Task<int>? ExecuteTask => base.ExecuteTask as Task<int>;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return WaitForStartAndInvokeAsync(stoppingToken);
    }

    private async Task WaitForApplicationStarted(CancellationToken cancelToken)
    {
        TaskCompletionSource<object?> appStarted = new();
        using var startedReg = lifetime.ApplicationStarted
            .Register(SetTaskComplete, appStarted);
        using var preStartCancelReg = cancelToken
            .Register(SetTaskCanceled, appStarted);

        await appStarted.Task
            .ConfigureAwait(continueOnCapturedContext: false);

        static void SetTaskComplete(object? state)
        {
            var tcs = (TaskCompletionSource<object?>)state!;
            tcs.TrySetResult(default);
        }

        static void SetTaskCanceled(object? state)
        {
            var tcs = (TaskCompletionSource<object?>)state!;
            tcs.TrySetCanceled(
                CancellationToken.None
            );
        }
    }

    private async Task<int> WaitForStartAndInvokeAsync(CancellationToken cancelToken)
    {
        await WaitForApplicationStarted(cancelToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        try 
        {
            int result = await invocation.InvokeAsync(cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
            return result;
        }
        finally
        {
            // If the application is not already shut down or shutting down,
            // make sure that application is shutting down now.
            if (!lifetime.ApplicationStopping.IsCancellationRequested)
            {
                lifetime.StopApplication();
            }
        }
    }
}