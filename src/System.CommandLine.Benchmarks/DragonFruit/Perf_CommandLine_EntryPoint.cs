// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Benchmarks.Helpers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.DragonFruit
{
    /// <summary>
    /// This is an end-to-end benchmark that measures the performance of
    /// <see cref="System.CommandLine.DragonFruit.CommandLine.ExecuteAssemblyAsync"/>
    /// including the execution of Main method per se.
    /// </summary>
    [BenchmarkCategory(Categories.DragonFruit)]
    [InvocationCount(3000)]
    public class Perf_CommandLine_EntryPoint
    {
        private Assembly _testAssembly;
        private string _testAssemblyFilePath;
        private string _testAssemblyXmlDocsFilePath;

        /// <remarks>
        /// For classesCount == 2, methodsPerClassCount == 3 will return:
        /// <code>
        /// namespace PerfTestApp
        /// {
        ///     public class ABCClass_0
        ///     {
        ///         public int Method_0() { return 0; }
        ///         public int Method_1() { return 0; }
        ///         public int Method_2() { return 0; }
        ///     }
        ///
        ///     public class ABCClass_1
        ///     {
        ///         public int Method_0() { return 0; }
        ///         ...
        ///         public int Method_2() { return 0; }
        ///     }
        /// }
        ///
        /// namespace PerfTestApp
        /// {
        ///     internal class Program
        ///     {
        ///         <summary>
        ///         User defined entry point.
        ///         </summary>
        ///         <param>....</param>
        ///         static int Main(int p1, string p2, bool p3) { return 0; }
        ///     } 
        /// }
        /// </code>
        /// </remarks>>
        private string GenerateTestAssemblySourceCode(
            int classesCount,
            int methodsPerClassCount,
            string classNamePrefix = "ABCClass",
            string methodNamePrefix = "Method"
        )
        {
            IEnumerable<string> testMethodsCodeSnapshot =
                Enumerable
                    .Range(0, methodsPerClassCount)
                    .Select(i => $"public int {methodNamePrefix}_{i}() {{ return 0; }} ");

            IEnumerable<string> testClassesCodeSnapshot =
                Enumerable
                    .Range(0, classesCount)
                    .Select(i =>
                        "namespace PerfTestApp { " +
                        $"public class {classNamePrefix}_{i} {{ {string.Concat(testMethodsCodeSnapshot)} }} " +
                        "} \n");

            string entryPointCodeSnapshot =
            @"namespace PerfTestApp {
                internal class Program {
                 
                    /// <summary>
                    /// Entry point summary...
                    /// </summary>
                    /// <param name=""p1"">....</param>
                    /// <param name=""p2"">....</param>
                    /// <param name=""p3"">....</param>
                    static int Main(int p1, string p2, bool p3) { return 0; } 
                } 
              }
             ";

            return string.Concat(testClassesCodeSnapshot) + entryPointCodeSnapshot;
        }

        private string CreateTestAssemblyInTempFile(int classesCount, int methodsPerClassCount)
        {
            string testSourceCode = GenerateTestAssemblySourceCode(classesCount, methodsPerClassCount);
            return Utils.CreateTestAssemblyInTempFileFromString(testSourceCode,
                new[]
                {
                    typeof(object).GetTypeInfo().Assembly.Location,
                    typeof(Enumerable).GetTypeInfo().Assembly.Location,
                });
        }

        public IEnumerable<(int classesCount, int methodsPerClassCount)> ValuesForTestAssemblySize
            => new[] {
                (classesCount: 1,   methodsPerClassCount: 1),
                (classesCount: 10,  methodsPerClassCount: 10),
                (classesCount: 100, methodsPerClassCount: 100)
            };

        [ParamsSource(nameof(ValuesForTestAssemblySize))]
        public (int classesCount, int methodsPerClassCount) TestAssemblySize;

        [GlobalSetup]
        public void Setup()
        {
            _testAssemblyFilePath = CreateTestAssemblyInTempFile(
                TestAssemblySize.classesCount,
                TestAssemblySize.methodsPerClassCount);

            _testAssembly = Assembly.Load(File.ReadAllBytes(_testAssemblyFilePath));
            _testAssemblyXmlDocsFilePath = _testAssemblyFilePath.Replace(".dll", ".xml");
        }

        [Benchmark(Description = "ExecuteAssemblyAsync entry point search.")]
        public Task SearchForStartingPointUsingReflection()
               => System.CommandLine.DragonFruit.CommandLine.ExecuteAssemblyAsync(
                    _testAssembly,
                    new string[] { },
                    null,
                    _testAssemblyXmlDocsFilePath);

        [Benchmark(Description = "ExecuteAssemblyAsync explicit entry point.")]
        public Task SearchForStartingPointWhenGivenEntryPointClass()
            => System.CommandLine.DragonFruit.CommandLine.ExecuteAssemblyAsync(
                _testAssembly,
                new string[] { },
                "PerfTestApp.Program",
                _testAssemblyXmlDocsFilePath);

        [GlobalCleanup]
        public void Cleanup()
        {
            File.Delete(_testAssemblyFilePath);
            File.Delete(_testAssemblyXmlDocsFilePath);
        }
    }
}
