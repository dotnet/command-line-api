// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using System.Linq;
using BenchmarkDotNet.Configs;
using System.Collections.Immutable;
using System.IO;

namespace System.CommandLine.Benchmarks
{
    class Program
    {
        static int Main(string[] args)
        {
            var result = BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
#if DEBUG
                .Run(args, new DebugInProcessConfig());
            Console.ReadLine();
#else
                .Run(args, RecommendedConfig.Create(
                    artifactsPath: new DirectoryInfo(Path.Combine(
                        Path.GetDirectoryName(typeof(Program).Assembly.Location), "BenchmarkDotNet.Artifacts")),
                    mandatoryCategories: ImmutableHashSet.Create(Categories.CommandLine, Categories.DragonFruit)));
#endif
            // an empty summary means that initial filtering and validation did not allow to run
            if (!result.Any())
                return 1;

            // if anything has failed, it's an error
            if (result.Any(summary => summary.HasCriticalValidationErrors || summary.Reports.Any(report => !report.BuildResult.IsBuildSuccess || !report.ExecuteResults.Any())))
                return 1;

            return 0;
        }
    }
}
