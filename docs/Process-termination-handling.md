# General

`System.CommandLine` supports performing actions when the invocation is stopped due to process termination, such as aborting an ongoing transaction or flushing some data to disk.

Process termination can be forceful, which means the process is terminated by the operating system and it doesn't get a chance to clean up. This is _killing_ a process.

Process termination can also be requested, for example when a user presses `Ctrl-C` on an interactive application or the system asks a service to terminate.

## Implementing termination handling

To add termination handling to a command, you must add a `CancellationToken` argument to the command handler. This token can then be passed along to async APIs that you might call from within your handler. Cancellation actions can also be added directly using the `CancellationToken.Register` method.

```c#
myCommand.Handler = CommandHandler.Create(async (IConsole console, CancellationToken token) =>
{
    try
    {
        using (var httpClient = new HttpClient())
        {
            await httpClient.GetAsync("http://www.example.com", token);
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

