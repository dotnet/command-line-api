using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation;

internal sealed class ProcessTerminationHandler : IDisposable
{
    private const int SIGINT_EXIT_CODE = 130;
    private const int SIGTERM_EXIT_CODE = 143;
        
    internal readonly TaskCompletionSource<int> ProcessTerminationCompletionSource;
    private readonly CancellationTokenSource _cts;
    private readonly Task<int> _startedInvocation;
    private readonly TimeSpan _processTerminationTimeout;
    private readonly IDisposable? _sigIntRegistration, _sigTermRegistration;
        
    internal ProcessTerminationHandler(
        CancellationTokenSource cts, 
        Task<int> startedInvocation,
        TimeSpan processTerminationTimeout)
    {
        ProcessTerminationCompletionSource = new ();
        _cts = cts;
        _startedInvocation = startedInvocation;
        _processTerminationTimeout = processTerminationTimeout;

#if  NET7_0_OR_GREATER
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
        if (_sigIntRegistration is not null)
        {
            _sigIntRegistration.Dispose();
            _sigTermRegistration!.Dispose();
        }
        else
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;    
        }
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
        try
        {
            _cts.Cancel();
        }
        catch (Exception ex)
        {
            ProcessTerminationCompletionSource.SetException(ex);
            return;
        }
        
        if (!_startedInvocation.Wait(_processTerminationTimeout))
        {
            ProcessTerminationCompletionSource.SetResult(forcedTerminationExitCode);
        }
    }
}