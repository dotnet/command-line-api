using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.ManualTest.AbandonOnRepeatCancellation
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var command = new RootCommand
            {
                Handler = CommandHandler.Create(
                async (IConsole console, CancellationToken cancelToken) =>
                {
                    console.Out.WriteLine("Invocation started. Press Ctrl+C to request cancellation");
                    using var cancelReg = cancelToken.Register(state =>
                    {
                        var console = (IConsole)state;
                        console.Out.WriteLine("Cancellation requested. Press Ctrl+C again, to abandon invocation.");
                    }, console);
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (true)
                    {
                        console.Out.WriteLine($"Time since start: {stopwatch.Elapsed}, Is cancellation requested: {cancelToken.IsCancellationRequested}");
                        await Task.Delay(TimeSpan.FromSeconds(0.75))
                            .ConfigureAwait(continueOnCapturedContext: false);
                    }
                }),
            };
            var parser = new CommandLineBuilder(command)
                .CancelOnProcessTermination()
                .AbandonOnRepeatCancellation()
                .Build();
            try
            {
                await parser
                    .InvokeAsync(args ?? Array.Empty<string>())
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Invocation was cancelled or abandoned.");
            }

            Console.WriteLine($"Back in {nameof(Main)}, waiting 5 seconds to exit.");
            await Task.Delay(TimeSpan.FromSeconds(5))
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
