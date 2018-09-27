using System;
using System.CommandLine.Rendering;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace RenderingPlayground
{
    internal class ProcessesView : ConsoleView<Process[]>
    {
        public ProcessesView(ConsoleRenderer writer, Region region) : base(writer, region)
        {
            writer.Formatter
                  .AddFormatter<TimeSpan>(t => new ContentSpan(t.ToString(@"hh\:mm\:ss")));
        }

        protected override void OnRender(Process[] processes)
        {
            WriteLine();

            WriteLine("Processes");

            WriteLine();

            //            RenderTable(processes.OrderByDescending(p => p.PrivateMemorySize64).Take(50),
            //                        table => {
            //                            table.RenderColumn("PID".Underline(),
            //                                               p => p.Id);

            //                            table.RenderColumn("COMMAND".Underline(),
            //                                               p => Name(p));

            //                            table.RenderColumn("TIME".Underline(),
            //                                               p => p.PrivilegedProcessorTime);

            //                            table.RenderColumn("#TH".Underline(),
            //                                               p => p.Threads.Count);

            //                            table.RenderColumn("MEM".Underline(),
            //                                               p => p.PrivateMemorySize64.Abbreviate());

            //                            table.RenderColumn("CPU".Underline(),
            //                                               p => {
            //#pragma warning disable CS0618 // Type or member is obsolete
            //                                                   var usage = p.TrackCpuUsage().First();
            //#pragma warning restore CS0618 // Type or member is obsolete
            //                                                   return $"{usage.UsageTotal:P}";
            //                                               });
            //                        });

            //FormattableString Name(Process p)
            //{
            //    if (!p.Responding)
            //    {
            //        return $"{ForegroundColorSpan.Rgb(180, 0, 0)}{p.ProcessName}{ForegroundColorSpan.Reset}";
            //    }

            //    return $"{p.ProcessName}";
            //}
        }
    }

    internal static class IntExtensions
    {
        private static readonly string[] _suffixes = {
            "b",
            "K",
            "M",
            "G",
            "T"
        };

        public static string Abbreviate(this long value)
        {
            var i = 0;
            var decimalValue = (decimal)value;

            while (Math.Round(decimalValue, 1) >= 1000)
            {
                decimalValue /= 1024;
                i++;
            }

            return $"{decimalValue:n1}{_suffixes[i]}";
        }

        public static DateTime StartTime = DateTime.UtcNow;

        public static IObservable<ProcessorTime> TrackCpuUsage(this Process process)
        {
            var processorCount = Environment.ProcessorCount;
            var trackingStartedAt = process.TotalProcessorTime;
            var lastCheckedAt = DateTime.UtcNow;
            var previousCpuTime = new TimeSpan(0);
            
            return Observable.Start(() => 
            {
                var currentCpuTime = process.TotalProcessorTime - trackingStartedAt;
            
                var usageSinceLastCheck = (currentCpuTime - previousCpuTime).TotalSeconds /
                                          (processorCount * DateTime.UtcNow.Subtract(lastCheckedAt).TotalSeconds);
            
                var usageTotal = currentCpuTime.TotalSeconds /
                                 (processorCount * DateTime.UtcNow.Subtract(StartTime).TotalSeconds);
            
                lastCheckedAt = DateTime.UtcNow;
            
                previousCpuTime = currentCpuTime;
                
                return new ProcessorTime(usageSinceLastCheck, usageTotal);
            }).Delay(TimeSpan.FromSeconds(1)).Repeat();

            //return Observable.Create<ProcessorTime>(observer =>
            //    {
            //        var currentCpuTime = process.TotalProcessorTime - trackingStartedAt;
            //
            //        var usageSinceLastCheck = (currentCpuTime - previousCpuTime).TotalSeconds /
            //                                  (processorCount * DateTime.UtcNow.Subtract(lastCheckedAt).TotalSeconds);
            //
            //        var usageTotal = currentCpuTime.TotalSeconds /
            //                         (processorCount * DateTime.UtcNow.Subtract(StartTime).TotalSeconds);
            //
            //        lastCheckedAt = DateTime.UtcNow;
            //
            //        previousCpuTime = currentCpuTime;
            //
            //        observer.OnNext(new ProcessorTime(usageSinceLastCheck, usageTotal));
            //
            //        return Disposable.Empty;
            //    })
            //    .Concat(Observable.Empty<ProcessorTime>().Delay(TimeSpan.FromSeconds(1)))
            //    .Repeat();
        }
    }

    internal class ProcessorTime
    {
        public double UsageSinceLastCheck { get; }
        public double UsageTotal { get; }

        public ProcessorTime(double usageSinceLastCheck, double usageTotal)
        {
            UsageSinceLastCheck = usageSinceLastCheck;
            UsageTotal = usageTotal;
        }
    }
}
