using System;
using System.Collections.Generic;
using System.CommandLine.Views;
using FluentAssertions;
using FluentAssertions.Extensions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class OutputRendererTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole console;
        private readonly ConsoleWriter consoleWriter;

        #region command line data and sample for "top" 

        private readonly ProcessInfo[] _processes = {
            new ProcessInfo(5747, "Terminal", 18.0, TimeSpan.Parse("00:00:42.82"), 8, 3, 261, 23_000_000, 496_000, 1_360_000, 5747, 1, "sleeping", 0, true, 1916, 0.06615, 0.69163,
                            501, 142717, 811),
            new ProcessInfo(5683, "WindowServer", 6.8, TimeSpan.Parse("00:20:04:23"), 5, 2, 1685, 62_000_000, 3_920_000, 75_000_000, 5683, 1, "sleeping", 0, true, 1, 0.69163,
                            0.00000,
                            88, 10701967, 20155),
            new ProcessInfo(48166, "top", 4.9, TimeSpan.Parse("00:00:07.95"), 1, 0, 24, 4_848_000, 0, 0, 48166, 48153, "running", 0, true, 1, 0.00000, 0.00000, 0, 44504, 111),
            new ProcessInfo(0, "kernel_task", 4.7, TimeSpan.Parse("00:17:05:49"), 147, 0, 2, 1_025_000_000, 0, 0, 0, 0, "running", 0, false, 0, 0.00000, 0.00000, 0, 191267, 0),
            new ProcessInfo(303, "mds_stores", 4.7, TimeSpan.Parse("00:54:45.15"), 5, 3, 80, 28_000_000, 1_872_000, 156_000_000, 303, 1, "sleeping", 0, true, 1, 0.00000, 4.66808,
                            0,
                            31393460, 539648),
            new ProcessInfo(47422, "mdworker", 3.5, TimeSpan.Parse("00:00:17.72"), 4, 1, 75, 29_000_000, 0, 30_000_000, 47422, 1, "sleeping", 0, true, 1, 4.52446, 0.00000, 501,
                            225598, 300),
            new ProcessInfo(113, "hidd", 2.9, TimeSpan.Parse("00:53:33.12"), 6, 2, 241, 2_220_000, 0, 3_628_000, 113, 1, "sleeping", 0, true, 1, 0.09521, 0.00000, 261, 1950651,
                            216),
            new ProcessInfo(140, "coreaudiod", 1.6, TimeSpan.Parse("00:58:14.26"), 7, 2, 307, 2_188_000, 0, 3_796_000, 140, 1, "sleeping", 0, true, 1, 0.00000, 0.00000, 202,
                            541103,
                            258),
        };

        private readonly string _topSampleOutput = @"
Processes: 8 total, 2 running, 6 sleeping, 183 threads                                                                                    22:27:52
Load Avg: 1.80, 1.92, 2.06  CPU usage: 6.47% user, 3.76% sys, 89.75% idle  SharedLibs: 147M resident, 49M data, 32M linkedit.
MemRegions: 109904 total, 2311M resident, 68M private, 793M shared. PhysMem: 8102M used (2150M wired), 89M unused.
VM: 1586G vsize, 1113M framework vsize, 67722816(0) swapins, 71848748(0) swapouts.   Networks: packets: 5742250/5467M in, 5175598/572M out.
Disks: 33227518/502G read, 16839665/472G written.

PID    COMMAND      %CPU TIME     #TH   #WQ  #PORT MEM    PURG   CMPRS  PGRP  PPID  STATE    BOOSTS          %CPU_ME %CPU_OTHRS UID  FAULTS    COW
5747   Terminal     18.0 00:42.82 8     3    261   23M+   496K   1360K  5747  1     sleeping *0[1916]        0.06615 0.69163    501  142717+   811
5683   WindowServer 6.8  20:04:23 5     2    1685- 62M    3920K  75M    5683  1     sleeping *0[1]           0.69163 0.00000    88   10701967+ 20155
48166  top          4.9  00:07.95 1     0    24    4848K  0B     0B     48166 48153 running  *0[1]           0.00000 0.00000    0    44504+    111
0      kernel_task  4.7  17:05:49 147   0    2     1025M+ 0B     0B     0     0     running   0[0]           0.00000 0.00000    0    191267    0
303    mds_stores   4.7  54:45.15 5     3    80    28M+   1872K  156M-  303   1     sleeping *0[1]           0.00000 4.66808    0    31393460+ 539648
47422  mdworker     3.5  00:17.72 4     1    75    29M+   0B     30M-   47422 1     sleeping *0[1]           4.52446 0.00000    501  225598+   300
113    hidd         2.9  53:33.12 6     2    241   2220K  0B     3628K  113   1     sleeping *0[1]           0.09521 0.00000    261  1950651+  216
140    coreaudiod   1.6  58:14.26 7     2    307   2188K  0B     3796K  140   1     sleeping *0[1]           0.00000 0.00000    202  541103    258";

        #endregion

        public OutputRendererTests(ITestOutputHelper output)
        {
            _output = output;

            console = new TestConsole {
                WindowWidth = 150
            };

            consoleWriter = new ConsoleWriter(console);
        }

        [Fact]
        public void Output_can_be_formatted_based_on_type_specific_formatters_when_type_is_rendered_directly()
        {
            consoleWriter.AddFormatter<TimeSpan>((ts, writer) => {
                writer.Write($"{ts.TotalSeconds} seconds");
            });

            var view = new AnonymousView<TimeSpan>(
                consoleWriter,
                (value, writer) => writer.Write(value));

            view.Render(21.Seconds());

            console.Out.ToString().Should().Be("21 seconds");
        }

        [Fact]
        public void Output_can_be_formatted_based_on_type_specific_formatters_when_type_is_embedded_in_interpolated_string()
        {
            consoleWriter.AddFormatter<TimeSpan>((ts, writer) => {
                writer.Write($"{ts.TotalSeconds} seconds");
            });

            var view = new AnonymousView<TimeSpan>(
                consoleWriter,
                (value, writer) => writer.Write($"{value}"));

            view.Render(21.Seconds());

            console.Out.ToString().Should().Be("21 seconds");
        }

        [Fact]
        public void Format_strings_are_honored_when_type_is_rendered_directly()
        {
            consoleWriter.AddFormatter<TimeSpan>((ts, writer) => {
                writer.Write($"{ts.TotalSeconds:F2} seconds");
            });

            var view = new AnonymousView<TimeSpan>(
                consoleWriter,
                (value, writer) => writer.Write(value));

            view.Render(21.Seconds());

            console.Out.ToString().Should().Be("21.00 seconds");
        }

        [Fact]
        public void Format_strings_are_honored_when_type_is_embedded_in_interpolated_string()
        {
            consoleWriter.AddFormatter<TimeSpan>((ts, writer) => {
                writer.Write($"{ts.TotalSeconds:F2} seconds");
            });

            var view = new AnonymousView<TimeSpan>(
                consoleWriter,
                (value, writer) => writer.Write($"{value}"));

            view.Render(21.Seconds());

            console.Out.ToString().Should().Be("21.00 seconds");
        }

        [Fact]
        public void Table_view_concept_1()
        {
            var view = new ProcessesTableView_Concept1(consoleWriter);

            view.Render(_processes);

            _output.WriteLine(console.Out.ToString());
        }

        [Fact]
        public void composition_sandbox()
        {
            var view = new ProcessListView(consoleWriter);

            view.Render(_processes);

            _output.WriteLine(console.Out.ToString());
        }
    }

    public class AnonymousView<T> : ConsoleView<T>
    {
        private readonly Action<T, IConsoleWriter> render;

        public AnonymousView(
            IConsoleWriter writer,
            Action<T, IConsoleWriter> render) : base(writer)
        {
            this.render = render ?? throw new ArgumentNullException(nameof(render));
        }

        public override void Render(T value) => render(value, ConsoleWriter);
    }

    public class ProcessInfo
    {
        public ProcessInfo(
            int processId,
            string command,
            double cpuPercentage,
            TimeSpan executionTime,
            int numberOfThreads,
            int workQueue,
            int port,
            int internalMemorySize,
            int purgeableMemorySize,
            int compressedDataBytes,
            int processGroupId,
            int parentProcessId,
            string state,
            int numberOfBoosts,
            bool processWasAbleToSendBoosts,
            int numberOfBoostTransitions,
            double cpuMe,
            double cpuOthers,
            int uid,
            int faults,
            int copyOnWriteFaults)
        {
            ProcessId = processId;
            Command = command;
            CpuPercentage = cpuPercentage;
            ExecutionTime = executionTime;
            NumberOfThreads = numberOfThreads;
            WorkQueue = workQueue;
            Port = port;
            InternalMemorySize = internalMemorySize;
            PurgeableMemorySize = purgeableMemorySize;
            CompressedDataBytes = compressedDataBytes;
            ProcessGroupId = processGroupId;
            ParentProcessID = parentProcessId;
            State = state;
            NumberOfBoosts = numberOfBoosts;
            ProcessWasAbleToSendBoosts = processWasAbleToSendBoosts;
            NumberOfBoostTransitions = numberOfBoostTransitions;
            CpuMe = cpuMe;
            CpuOthers = cpuOthers;
            Uid = uid;
            Faults = faults;
            CopyOnWriteFaults = copyOnWriteFaults;
        }

        public int ProcessId { get; } //  PID
        public string Command { get; } //  COMMAND
        public double CpuPercentage { get; } //  %CPU
        public TimeSpan ExecutionTime { get; } // TIME
        public int NumberOfThreads { get; } // #TH
        public int WorkQueue { get; } // #WQ
        public int Port { get; } // #PORT
        public int InternalMemorySize { get; } //  MEM
        public int PurgeableMemorySize { get; } //  PURG
        public int CompressedDataBytes { get; } //  CMPRS
        public int ProcessGroupId { get; } //  PGRP
        public int ParentProcessID { get; } //  PPID
        public string State { get; } // STATE
        public int NumberOfBoosts { get; } // BOOSTS
        public bool ProcessWasAbleToSendBoosts { get; } // BOOSTS, marked by an asterisk in the BOOSTS column
        public int NumberOfBoostTransitions { get; } // BOOSTS, in brackets under BOOSTS column
        public double CpuMe { get; } // %CPU_ME
        public double CpuOthers { get; } // %CPU_OTHRS
        public int Uid { get; } // UID
        public int Faults { get; } // FAULTS
        public int CopyOnWriteFaults { get; } // COW
    }

    public class ProcessesTableView_Concept1 : ConsoleView<IReadOnlyCollection<ProcessInfo>>
    {
        public ProcessesTableView_Concept1(IConsoleWriter writer) : base(writer)
        {
        }

        public override void Render(IReadOnlyCollection<ProcessInfo> processes)
        {
            this.RenderTable(
                items: processes,
                table: table => {
                    table.RenderColumn("PID", p => p.ProcessId);
                    table.RenderColumn("COMMAND", p => p.Command);
                    table.RenderColumn("%CPU", p => p.CpuPercentage);
                    table.RenderColumn("TIME", p => p.ExecutionTime);
                    table.RenderColumn("#TH", p => p.NumberOfThreads);
                    table.RenderColumn("#WQ", p => p.WorkQueue);
                    table.RenderColumn("#PORT", p => p.Port);
                    table.RenderColumn("MEM", p => p.InternalMemorySize);
                    table.RenderColumn("PURG", p => p.PurgeableMemorySize);
                    table.RenderColumn("CMPRS", p => p.CompressedDataBytes);
                    table.RenderColumn("PGRP", p => p.ProcessGroupId);
                    table.RenderColumn("PPID", p => p.ParentProcessID);
                    table.RenderColumn("STATE", p => p.State);
                    table.RenderColumn("BOOSTS", Boosts);
                    table.RenderColumn("%CPU_ME", p => p.CpuMe);
                    table.RenderColumn("%CPU_OTHRS", p => p.CpuOthers);
                    table.RenderColumn("UID", p => p.Uid);
                    table.RenderColumn("FAULTS", p => p.Faults);
                    table.RenderColumn("COW", p => p.CopyOnWriteFaults);
                    table.RenderColumn("PID", p => p.ProcessId);
                });

            FormattableString Boosts(ProcessInfo p) =>
                $"{(p.ProcessWasAbleToSendBoosts ? "*" : "")}{p.NumberOfBoosts}[{p.NumberOfBoostTransitions}]";
        }
    }

    public class ProcessesSummaryView : ConsoleView<IEnumerable<ProcessInfo>>
    {
        public ProcessesSummaryView(IConsoleWriter writer) : base(writer)
        {
        }

        public override void Render(IEnumerable<ProcessInfo> processes)
        {
            var total = processes.Count();
            var running = processes.Count(v => v.State == "running");
            var sleeping = processes.Count(v => v.State == "sleeping");
            var threads = processes.Sum(v => v.NumberOfThreads);

            Render(
                $@"
Processes: {total} total, {running} running, {sleeping} sleeping, {threads} threads                                                                                    22:27:52
Load Avg: 1.80, 1.92, 2.06  CPU usage: 6.47% user, 3.76% sys, 89.75% idle  SharedLibs: 147M resident, 49M data, 32M linkedit.
MemRegions: 109904 total, 2311M resident, 68M private, 793M shared. PhysMem: 8102M used (2150M wired), 89M unused.
VM: 1586G vsize, 1113M framework vsize, 67722816(0) swapins, 71848748(0) swapouts.   Networks: packets: 5742250/5467M in, 5175598/572M out.
Disks: 33227518/502G read, 16839665/472G written.
");
        }
    }

    public class ProcessListView : ConsoleView<IEnumerable<ProcessInfo>>
    {
        public ProcessListView(IConsoleWriter writer) : base(writer)
        {
            ProcessesSummary = new ProcessesSummaryView(ConsoleWriter);
            ProcessDetail = new ProcessDetail(ConsoleWriter);
        }

        public override void Render(IEnumerable<ProcessInfo> processes)
        {
            ProcessesSummary.Render(processes);

            Render(
                $"PID{Column}COMMAND{Column}%CPU{Column}TIME{Column}#TH{Column}#WQ{Column}#PORT{Column}MEM{Column}PURG{Column}CMPRS{Column}PGRP{Column}PPID{Column}STATE{Column}BOOSTS{Column}%CPU_ME{Column}%CPU_OTHRS{Column}UID{Column}FAULTS{Column}COW");

            foreach (var result in processes)
            {
                ProcessDetail.Render(result);
            }
        }

        public ProcessDetail ProcessDetail { get; }

        public ProcessesSummaryView ProcessesSummary { get; }
    }

    public class ProcessDetail : ConsoleView<ProcessInfo>
    {
        public override void Render(ProcessInfo p)
        {
            ConsoleWriter.Write($"{p.ProcessId}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.Command}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.CpuPercentage}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.ExecutionTime}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.NumberOfThreads}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.WorkQueue}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.Port}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.InternalMemorySize}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.PurgeableMemorySize}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.CompressedDataBytes}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.ProcessGroupId}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.ParentProcessID}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.State}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.ProcessWasAbleToSendBoosts}{p.NumberOfBoosts}[{p.NumberOfBoostTransitions}]");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.CpuMe}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.CpuOthers}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.Uid}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.Faults}");
            ConsoleWriter.Write($"{Column}");
            ConsoleWriter.Write($"{p.CopyOnWriteFaults}");
            ConsoleWriter.WriteLine();
        }

        public ProcessDetail(IConsoleWriter writer) : base(writer)
        {
        }
    }
}
