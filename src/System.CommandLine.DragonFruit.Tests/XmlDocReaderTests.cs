// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
            public static void Main(bool verbose = false, string flavor = null, int? count = 0)
            {
            }

            public static void MainWithoutParam()
            {
            }
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
        <member name=""M:System.CommandLine.DragonFruit.Tests." + nameof(XmlDocReaderTests) + @".Program.Main(System.Boolean,System.String,System.Nullable{System.Int32})"">
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
            Action<bool, string, int?> action = Program.Main;
            var reader = new StringReader(xml);
            XmlDocReader.TryLoad(reader, out var docReader).Should().BeTrue();

            docReader.TryGetMethodDescription(action.Method, out var helpMetadata).Should().BeTrue();
            helpMetadata.Description.Should().Be("Hello");
            helpMetadata.ParameterDescriptions["verbose"].Should().Be("Show verbose output");
            helpMetadata.ParameterDescriptions["flavor"].Should().Be("Which flavor to use");
            helpMetadata.ParameterDescriptions["count"].Should().Be("How many smoothies?");
        }

        [Fact]
        public void It_finds_member_without_param()
        {
            const string xml = @"<?xml version=""1.0""?>
<doc>
    <assembly>
        <name>DragonFruit</name>
    </assembly>
    <members>
        <member name=""M:System.CommandLine.DragonFruit.Tests." + nameof(XmlDocReaderTests) + @".Program.MainWithoutParam"">
            <summary>
            Hello
            </summary>
        </member>
    </members>
</doc>
";
            Action action = Program.MainWithoutParam;
            var reader = new StringReader(xml);
            XmlDocReader.TryLoad(reader, out var docReader).Should().BeTrue();

            docReader.TryGetMethodDescription(action.Method, out var helpMetadata).Should().BeTrue();
            helpMetadata.Description.Should().Be("Hello");
        }
    }
}
