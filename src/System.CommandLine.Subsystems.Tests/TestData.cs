// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Reflection;

namespace System.CommandLine.Subsystems.Tests;

internal class TestData
{
    internal static readonly string? AssemblyVersionString = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                                     ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                     ?.InformationalVersion;

    internal class Version : IEnumerable<object[]>
    {
        // This data only works if the CLI has a --version with a -v alias and also has a -x option
        private readonly List<object[]> _data =
        [
            ["--version", true],
            ["-v", true],
            ["-vx", true],
            ["-xv", true],
            ["-x", false],
            [null, false],
            ["", false],
        ];

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class Directive : IEnumerable<object[]>
    {
        private readonly List<object[]> _data =
        [
            ["[diagram]", true, false, null],
            ["[other:Hello]", false, true, "Hello"],
            ["[diagram] x", true, false, null],
            ["[diagram] -o", true, false, null],
            ["[diagram] -v", true, false, null],
            ["[diagram] x -v", true, false, null],
            ["[diagramX]", false, false, null],
            ["[diagram] [other:Hello]", true, true, "Hello"],
            ["x", false, false, null],
            ["-o", false, false, null],
            ["x -x", false, false, null],
            [null, false, false, null],
            ["", false, false, null],
            //["[diagram] [other Goodbye]", true, true, "Goodbye"],This is a new test that demos new feature, but is also broken
        ];

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
