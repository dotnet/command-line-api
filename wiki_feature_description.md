# General

`System.CommandLine` supports performing actions when the invocation is being stopped due to process termination. For example, aborting an ongoing transaction, or flushing some data to disk.

Process termination can be forcefull, which means the process is terminated by the OS and it doesn't get a chance to cleanup. This is _killing_ a process.

Process termination can also be requested. For example, the user presses Control-C on an interactive application, or the system asks a service to terminate.

# Implementing termination handling

To add termination handling to a command, you must add a `CancellationToken` argument to the command handler. This token can be passed to async APIs. Cancellation actions can also be added directly using the `CancellationToken.Register` method.

```c#
CommandHandler.Create(async (IConsole console, CancellationToken ct) =>
{
    try
    {
        using (var httpClient = new HttpClient())
        {
            await httpClient.GetAsync("http://www.example.com", ct);
        }
        return 0;
    }
    catch (OperationCanceledException)
    {
        console.Error.WriteLine("The operation was aborted");
        return 1;
    }
});
```

To trigger cancellation, the `CancelOnProcessTermination` middleware must be added to the `CommandLineBuilder`.

```c#
var builder =  new CommandLineBuilder()
    . ...
    .CancelOnProcessTermination()
    . ...
```