// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace System.CommandLine.Benchmarks.Helpers
{
    internal static class Utils
    {
        internal static string GetInputFullFilePath(string name)
            => Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "Input", name);

        internal static string CreateTestAssemblyInTempFileFromFile(string testCsFilePath, IEnumerable<string> references)
        {
            var testSourceCode = File.ReadAllText(GetInputFullFilePath(testCsFilePath));
            return CreateTestAssemblyInTempFileFromString(testSourceCode, references);
        }

        internal static string CreateTestAssemblyInTempFileFromString(string sourceCode, IEnumerable<string> references)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            string randomAssemblyName = Path.GetRandomFileName();
            var compiler = CSharpCompilation.Create(
                randomAssemblyName,
                new[] { syntaxTree },
                references.Select(r => MetadataReference.CreateFromFile(r)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            string pathToAssemblyFile = Path.Combine(Path.GetTempPath(), randomAssemblyName + ".dll");
            string pathToAssemblyXmlDocsFile = Path.Combine(Path.GetTempPath(), randomAssemblyName + ".xml");
            EmitResult result = compiler.Emit(pathToAssemblyFile, null, pathToAssemblyXmlDocsFile);

            if (!result.Success)
            {
                throw new Exception("Invalid test assembly code.");
            }

            return pathToAssemblyFile;
        }

        public static CliConfiguration CreateConfiguration(this IEnumerable<CliOption> symbols)
        {
            var rootCommand = new CliRootCommand();

            foreach (var symbol in symbols)
            {
                rootCommand.Add(symbol);
            }

            return new CliConfiguration(rootCommand);
        }
    }
}
