using System;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Diagnostics;
using System.Reactive.Linq;

namespace RenderingPlayground
{
    internal class ProcessesView : StackLayoutView
    {
        public ProcessesView(Process[] processes)
        {
            var formatter = new TextSpanFormatter();
            formatter.AddFormatter<TimeSpan>(t => new ContentSpan(t.ToString(@"hh\:mm\:ss")));

            Add(new ContentView(""));
            Add(new ContentView("Processes"));
            Add(new ContentView(""));

            var table = new TableView<Process>
            {
                Items = processes
            };
            table.AddColumn(p => p.Id, new ContentView("PID".Underline()));
            table.AddColumn(p => Name(p), new ContentView("COMMAND".Underline()));
            table.AddColumn(p => p.PrivilegedProcessorTime, new ContentView("TIME".Underline()));
            table.AddColumn(p => p.Threads.Count, new ContentView("#TH".Underline()));
            table.AddColumn(p => p.PrivateMemorySize64.Abbreviate(), new ContentView("MEM".Underline()));
            table.AddColumn(p =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var usage = p.TrackCpuUsage().First();
#pragma warning restore CS0618 // Type or member is obsolete
                return $"{usage.UsageTotal:P}";
            }, new ContentView("CPU".Underline()));


            Add(table);

            FormattableString Name(Process p)
            {
                if (!p.Responding)
                {
                    return $"{ForegroundColorSpan.Rgb(180, 0, 0)}{p.ProcessName}{ForegroundColorSpan.Reset()}";
                }
                return $"{p.ProcessName}";
            }
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

            return Observable.ToObservable(GetTime()).Delay(TimeSpan.FromSeconds(1)).Repeat();

            IEnumerable<ProcessorTime> GetTime()
            {
                var currentCpuTime = process.TotalProcessorTime - trackingStartedAt;

                var usageSinceLastCheck = (currentCpuTime - previousCpuTime).TotalSeconds /
                                          (processorCount * DateTime.UtcNow.Subtract(lastCheckedAt).TotalSeconds);

                var usageTotal = currentCpuTime.TotalSeconds /
                                 (processorCount * DateTime.UtcNow.Subtract(StartTime).TotalSeconds);

                lastCheckedAt = DateTime.UtcNow;

                previousCpuTime = currentCpuTime;

                yield return new ProcessorTime(usageSinceLastCheck, usageTotal);
            }

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
