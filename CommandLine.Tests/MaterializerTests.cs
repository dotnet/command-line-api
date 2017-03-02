// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class MaterializerTests
    {
        private readonly ITestOutputHelper output;

        public MaterializerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void A_command_can_be_materialized_using_options_and_arguments()
        {
            var parser = new Parser(
                Command("move", "",
                        arguments: Accept.OneOrMoreArguments,
                        options: new[]
                        {
                            Option("-d|--destination", "", Accept.ExactlyOneArgument)
                        },
                        materialize: p => {
                            output.WriteLine(p.ToString()); 
                            
                            return new FileMoveOperation
                            {
                                Files = p.Arguments.Select(f => new FileInfo(f)).ToList(),
                                Destination = new DirectoryInfo(p["destination"].Arguments.Single())
                            };
                        }));

            var folder = new DirectoryInfo(Path.Combine("temp"));
            var file1 = new FileInfo(Path.Combine(folder.FullName, "the file.txt"));
            var file2 = new FileInfo(Path.Combine(folder.FullName, "the other file.txt"));

            var result = parser.Parse($@"move -d ""{folder}"" ""{file1}"" ""{file2}""");

            var fileMoveOperation = result["move"].Value<FileMoveOperation>();

            fileMoveOperation.Destination
                             .FullName
                             .Should()
                             .Be(folder.FullName);

            fileMoveOperation.Files
                             .Select(f => f.FullName)
                             .Should()
                             .BeEquivalentTo(file2.FullName,
                                             file1.FullName);
        }

        [Fact(Skip="Not implemented")]
        public void An_option_without_arguments_materializes_as_true_when_it_is_applied()
        {
            var command = Command("something", "",
                                  Accept.NoArguments,
                                  Option("-x", ""));

            var result = command.Parse("something -x");

            result["something"]["x"].Value<bool>().Should().BeTrue();
        }

        [Fact(Skip="Not implemented")]
        public void An_option_without_arguments_materializes_as_false_when_it_is_not_applied()
        {
            var command = Command("something", "", Option("-x", ""));

            var result = command.Parse("something");

            result["something"]["x"].Value<bool>().Should().BeFalse();
        }

        public class FileMoveOperation
        {
            public List<FileInfo> Files { get; set; }
            public DirectoryInfo Destination { get; set; }
        }
    }
}