// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Benchmarks.Helpers;
using System.CommandLine.DragonFruit;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace System.CommandLine.Benchmarks.DragonFruit
{
    /// <summary>
    /// Measures performance of initializing symbol descriptions from xml comments.
    /// <see cref="System.CommandLine.DragonFruit.XmlDocReader"/>
    /// </summary>
    [BenchmarkCategory(Categories.DragonFruit)]
    public class Perf_XmlDocReader
    {
        private readonly string _testAssemblyFilePath;
        private readonly string _testAssemblyXmlDocsFilePath;
        private readonly MethodInfo _mainMethodInfo;
        private readonly StreamReader _xmlDocsStreamReader;
        private XmlDocReader _xmlDocReaderSample1;

        public Perf_XmlDocReader()
        {
            _testAssemblyFilePath = Utils.CreateTestAssemblyInTempFileFromFile(
                "Sample1.Main.cs",
                new[]
                {
                    typeof(object).GetTypeInfo().Assembly.Location,
                    typeof(Enumerable).GetTypeInfo().Assembly.Location,
                    typeof(ParseResult).GetTypeInfo().Assembly.Location
                }
            );

            _testAssemblyXmlDocsFilePath = _testAssemblyFilePath.Replace(".dll", ".xml");

            var testAssembly = Assembly.Load(File.ReadAllBytes(_testAssemblyFilePath));

            _mainMethodInfo = testAssembly
                .GetType("RenderingPlayground.Program", false, false)
                .GetTypeInfo()
                .GetDeclaredMethod("Main");

            _xmlDocsStreamReader = new StreamReader(
                new MemoryStream(File.ReadAllBytes(_testAssemblyXmlDocsFilePath)
                )
            );
        }

        [Benchmark]
        public XmlDocReader TryLoad_Sample1()
        {
            // I experienced problems with [IterationSetup]/[IterationCleanup]
            // https://github.com/dotnet/BenchmarkDotNet/issues/1127
            // So I have ended up placing it here for now
            _xmlDocsStreamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            //
            XmlDocReader.TryLoad(_xmlDocsStreamReader, out var docReader);
            return docReader;
        }

        [GlobalSetup(Target = nameof(TryGetMethodDescription_Sample1))]
        public void SetupTryGetMethodDescription_Sample1()
        {
            _xmlDocReaderSample1 = TryLoad_Sample1();
        }

        [Benchmark]
        public CommandHelpMetadata TryGetMethodDescription_Sample1()
        {
            // I experienced problems with [IterationSetup]/[IterationCleanup]
            // https://github.com/dotnet/BenchmarkDotNet/issues/1127
            // So I have ended up placing it here for now
            _xmlDocsStreamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            //
            _xmlDocReaderSample1.TryGetMethodDescription(_mainMethodInfo, out var helpMetadata);
            return helpMetadata;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _xmlDocsStreamReader.Close();
            File.Delete(_testAssemblyFilePath);
            File.Delete(_testAssemblyXmlDocsFilePath);
        }
    }
}
