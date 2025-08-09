// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.IO;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;

namespace System.CommandLine.Benchmarks
{
    public static class RecommendedConfig
    {
        public static IConfig Create(DirectoryInfo artifactsPath, ImmutableHashSet<string> mandatoryCategories)
            => DefaultConfig.Instance
                .AddJob(Job.Default
                    .WithWarmupCount(1)
                    .WithIterationTime(TimeInterval.FromMilliseconds(250))
                    .WithMinIterationCount(15)
                    .WithMaxIterationCount(20)
                    .AsDefault())
                .WithArtifactsPath(artifactsPath.FullName)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddExporter(JsonExporter.Full)
                .AddColumn(StatisticColumn.Median, StatisticColumn.Min, StatisticColumn.Max);
    }
}
