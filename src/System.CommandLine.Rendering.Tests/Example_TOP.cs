// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Rendering.Tests
{
    public class Example_TOP
    {
        private readonly ITestOutputHelper _output;
        private readonly TestTerminal _terminal;
        private readonly ConsoleRenderer consoleRenderer;

        #region command line data and sample for "top" 

        public static readonly ProcessInfo[] Processes = {
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

        public Example_TOP(ITestOutputHelper output)
        {
            _output = output;

            _terminal = new TestTerminal {
                Width = 150
            };

            consoleRenderer = new ConsoleRenderer(
                _terminal,
                OutputMode.PlainText);
        }

        [Fact]
        public void EXAMPLE_Table_view_emulating_top()
        {
            consoleRenderer.Formatter.AddFormatter<TimeSpan>(t => new ContentSpan(t.ToString(@"hh\:mm\:ss")));

            var view = new ProcessesTableView(Processes);

            view.Render(consoleRenderer, new Region(0, 0, 200, 16));

            _output.WriteLine("\n\nVIEW emulating top output:\n");

            _output.WriteLine(_terminal.Out.ToString());

            _output.WriteLine("REAL top output:\n");

            _output.WriteLine(_topSampleOutput);
        }
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

    public class ProcessesTableView : TableView<ProcessInfo>
    {
        public ProcessesTableView(IReadOnlyList<ProcessInfo> processes)
        {
            Items = processes;

            AddColumn(p => p.ProcessId, "PID");
            AddColumn(p => p.Command, "COMMAND");
            AddColumn(p => p.CpuPercentage, "%CPU");
            AddColumn(p => p.ExecutionTime, "TIME");
            AddColumn(p => p.NumberOfThreads, "#TH");
            AddColumn(p => p.WorkQueue, "#WQ");
            AddColumn(p => p.Port, "#PORT");
            AddColumn(p => p.InternalMemorySize, "MEM");
            AddColumn(p => p.PurgeableMemorySize, "PURG");
            AddColumn(p => p.CompressedDataBytes, "CMPRS");
            AddColumn(p => p.ProcessGroupId, "PGRP");
            AddColumn(p => p.ParentProcessID, "PPID");
            AddColumn(p => p.State, "STATE");
            AddColumn(p => Boosts(p), "BOOSTS");
            AddColumn(p => p.CpuMe, "%CPU_ME");
            AddColumn(p => p.CpuOthers, "%CPU_Others");
            AddColumn(p => p.Uid, "UID");
            AddColumn(p => p.Faults, "FAULTS");
            AddColumn(p => p.CopyOnWriteFaults, "COW");

            FormattableString Boosts(ProcessInfo p) =>
                $"{(p.ProcessWasAbleToSendBoosts ? "*" : "")}{p.NumberOfBoosts}[{p.NumberOfBoostTransitions}]";
        }
    }

    public class ProcessesSummaryView : ContentView
    {
        public ProcessesSummaryView(IEnumerable<ProcessInfo> processes)
        {
            var total = processes.Count();
            var running = processes.Count(v => v.State == "running");
            var sleeping = processes.Count(v => v.State == "sleeping");
            var threads = processes.Sum(v => v.NumberOfThreads);

            Span = new ContentSpan($@"
Processes: {total} total, {running} running, {sleeping} sleeping, {threads} threads                                                                                    22:27:52
Load Avg: 1.80, 1.92, 2.06  CPU usage: 6.47% user, 3.76% sys, 89.75% idle  SharedLibs: 147M resident, 49M data, 32M linkedit.
MemRegions: 109904 total, 2311M resident, 68M private, 793M shared. PhysMem: 8102M used (2150M wired), 89M unused.
VM: 1586G vsize, 1113M framework vsize, 67722816(0) swapins, 71848748(0) swapouts.   Networks: packets: 5742250/5467M in, 5175598/572M out.
Disks: 33227518/502G read, 16839665/472G written.
");
        }
    }
}
