using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation;

internal sealed class ProcessTerminationHandler : IDisposable
{
    private const int SIGINT_EXIT_CODE = 130;
    private const int SIGTERM_EXIT_CODE = 143;
        
    internal readonly TaskCompletionSource<int> ProcessTerminationCompletionSource;
    private readonly CancellationTokenSource _handlerCancellationTokenSource;
    private readonly Task<int> _startedHandler;
    private readonly TimeSpan _processTerminationTimeout;
#if NET7_0_OR_GREATER
    private readonly IDisposable? _sigIntRegistration, _sigTermRegistration;
#endif
        
    internal ProcessTerminationHandler(
        CancellationTokenSource handlerCancellationTokenSource, 
        Task<int> startedHandler,
        TimeSpan processTerminationTimeout)
    {
        ProcessTerminationCompletionSource = new ();
        _handlerCancellationTokenSource = handlerCancellationTokenSource;
        _startedHandler = startedHandler;
        _processTerminationTimeout = processTerminationTimeout;

#if NET7_0_OR_GREATER // we prefer the new API as they allow for cancelling SIGTERM
        if (!OperatingSystem.IsAndroid() 
            && !OperatingSystem.IsIOS() 
            && !OperatingSystem.IsTvOS()
            && !OperatingSystem.IsBrowser())
        {
            _sigIntRegistration = PosixSignalRegistration.Create(PosixSignal.SIGINT, OnPosixSignal);
            _sigTermRegistration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, OnPosixSignal);
            return;
        }
#endif

        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    public void Dispose()
    {
#if NET7_0_OR_GREATER
        if (_sigIntRegistration is not null)
        {
            _sigIntRegistration.Dispose();
            _sigTermRegistration!.Dispose();
            return;
        }
#endif

        Console.CancelKeyPress -= OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;    
    }
        
#if NET7_0_OR_GREATER
    void OnPosixSignal(PosixSignalContext context)
    {
        context.Cancel = true;
            
        Cancel(context.Signal == PosixSignal.SIGINT ? SIGINT_EXIT_CODE : SIGTERM_EXIT_CODE);
    }
#endif

    void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;

        Cancel(SIGINT_EXIT_CODE);
    }

    void OnProcessExit(object? sender, EventArgs e) => Cancel(SIGTERM_EXIT_CODE);

    void Cancel(int forcedTerminationExitCode)
    {
        // request cancellation
        _handlerCancellationTokenSource.Cancel();

        try
        {
            // wait for the configured interval
            if (!_startedHandler.Wait(_processTerminationTimeout))
            {
                // if the handler does not finish within configured time,
                // use the completion source to signal forced completion (preserving native exit code)
                ProcessTerminationCompletionSource.SetResult(forcedTerminationExitCode);
            }
        }
        catch (AggregateException)
        {
            // The task was cancelled or an exception was thrown during the task execution.
        }
    }
}