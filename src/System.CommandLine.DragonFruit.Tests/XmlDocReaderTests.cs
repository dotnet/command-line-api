﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class XmlDocReaderTests
    {
        private class Program
        {
            public static void Main(bool verbose, string flavor, int count) { }
        }

        [Fact]
        public void It_finds_member_xml()
        {
            const string xml = @"<?xml version=""1.0""?>
<doc>
    <assembly>
        <name>DragonFruit</name>
    </assembly>
    <members>
        <member name=""M:System.CommandLine.DragonFruit.Tests." + nameof(XmlDocReaderTests) + @".Program.Main(System.Boolean,System.String,System.Int32)"">
            <summary>
            Hello
            </summary>
            <param name=""verbose"">Show verbose output</param>
            <param name=""flavor"">Which flavor to use</param>
            <param name=""count"">How many smoothies?</param>
        </member>
    </members>
</doc>
";
            Action<bool, string, int> action = Program.Main;
            var reader = new StringReader(xml);
            XmlDocReader.TryLoad(reader, out var docReader).Should().BeTrue();
            docReader.TryGetMethodDescription(action.Method, out var helpMetadata).Should().BeTrue();
            helpMetadata.Description.Should().Be("Hello");
            helpMetadata.TryGetParameterDescription("verbose", out var verboseDesc).Should().BeTrue();
            verboseDesc.Should().Be("Show verbose output");
            helpMetadata.TryGetParameterDescription("flavor", out var flavorDesc).Should().BeTrue();
            flavorDesc.Should().Be("Which flavor to use");
            helpMetadata.TryGetParameterDescription("count", out var countDesc).Should().BeTrue();
            countDesc.Should().Be("How many smoothies?");
        }
    }
}
