// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Benchmarks.Helpers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.DragonFruit
{
    /// <summary>
    /// This is an end-to-end benchmark that measures the performance of --help option.
    /// </summary>
    [BenchmarkCategory(Categories.DragonFruit)]
    [InvocationCount(3000)]
    public class Perf_CommandLine_Help
    {
        private Assembly _testAssembly;
        private string _testAssemblyFilePath;
        private string _testAssemblyXmlDocsFilePath;

        [GlobalSetup]
        public void Setup()
        {
            _testAssemblyFilePath = Utils.CreateTestAssemblyInTempFileFromFile(
                "Sample1.Main.cs",
                new[]
                {
                    typeof(object).GetTypeInfo().Assembly.Location,
                    typeof(Enumerable).GetTypeInfo().Assembly.Location,
                    typeof(System.CommandLine.ParseResult).GetTypeInfo().Assembly.Location
                }
            );
            _testAssembly = Assembly.Load(File.ReadAllBytes(_testAssemblyFilePath));
            _testAssemblyXmlDocsFilePath = _testAssemblyFilePath.Replace(".dll", ".xml");
        }

        [Benchmark(Description = "--help")]
        public Task SearchForStartingPointWhenGivenEntryPointClass_Help()
            => System.CommandLine.DragonFruit.CommandLine.ExecuteAssemblyAsync(
                _testAssembly,
                new[] { "--help" },
                null,
                _testAssemblyXmlDocsFilePath);

        [GlobalCleanup]
        public void Cleanup()
        {
            File.Delete(_testAssemblyFilePath);
            File.Delete(_testAssemblyXmlDocsFilePath);
        }
    }
}
