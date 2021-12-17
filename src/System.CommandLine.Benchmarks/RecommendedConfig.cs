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
#pragma warning disable CA1801 // Review unused parameters
        public static IConfig Create(DirectoryInfo artifactsPath, ImmutableHashSet<string> mandatoryCategories)
#pragma warning restore CA1801 // Review unused parameters
#pragma warning disable CA1062 // Validate arguments of public methods
            => DefaultConfig.Instance
                .With(Job.Default
                    .WithWarmupCount(1)
                    .WithIterationTime(TimeInterval.FromMilliseconds(250))
                    .WithMinIterationCount(15)
                    .WithMaxIterationCount(20)
                    .AsDefault())
                .WithArtifactsPath(artifactsPath.FullName)
                .With(MemoryDiagnoser.Default)
                .With(JsonExporter.Full)
                .With(StatisticColumn.Median, StatisticColumn.Min, StatisticColumn.Max)
                .WithSummaryStyle(SummaryStyle.Default.WithSizeUnit(SizeUnit.B));
#pragma warning restore CA1062 // Validate arguments of public methods
    }
}
