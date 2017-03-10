// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
                        arguments: Accept.OneOrMoreArguments()
                                         .MaterializeAs(p =>
                                         {
                                             output.WriteLine(p.ToString());

                                             return new FileMoveOperation
                                             {
                                                 Files = p.Arguments.Select(f => new FileInfo(f)).ToList(),
                                                 Destination = new DirectoryInfo(p["destination"].Arguments.Single())
                                             };
                                         }),
                        options: new[]
                        {
                            Option("-d|--destination", "", Accept.ExactlyOneArgument())
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

        [Fact]
        public void An_option_with_a_single_argument_materializes_as_the_argument_string_value_by_default()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", Accept.ExactlyOneArgument()));

            var result = command.Parse("the-command -x the-argument");

            result["the-command"]["x"]
                .Value()
                .Should()
                .Be("the-argument");
        }

        [Fact]
        public void An_option_with_multiple_arguments_materializes_as_a_sequence_of_strings_by_default()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", Accept.ZeroOrMoreArguments()));

            command.Parse("the-command -x arg1 arg2")["the-command"]["x"]
                   .Value()
                   .ShouldBeEquivalentTo(new[] { "arg1", "arg2" });

            command.Parse("the-command -x arg1")["the-command"]["x"]
                   .Value()
                   .ShouldBeEquivalentTo(new[] { "arg1" });
        }

        [Fact]
        public void An_option_without_arguments_materializes_as_true_when_it_is_applied()
        {
            var command = Command("something", "",
                                  Accept.NoArguments(),
                                  Option("-x", ""));

            var result = command.Parse("something -x");

            result["something"]["x"].Value<bool>().Should().BeTrue();
        }

        [Fact(Skip = "Not implemented yet")]
        public void An_option_without_arguments_materializes_as_false_when_it_is_not_applied()
        {
            var command = Command("something", "", Option("-x", ""));

            var result = command.Parse("something");

            result["something"]["x"].Value<bool>().Should().BeFalse();
        }

        [Fact]
        public void When_a_materializer_throws_then_an_informative_exception_message_is_given()
        {
            var command = Command("the-command", "",
                                  Option("-o|--one", "",
                                         Accept.ExactlyOneArgument()
                                               .MaterializeAs(o => int.Parse(o.Arguments.Single()))));

            var result = command.Parse("the-command -o not-an-int");

            Action getValue = () => result["the-command"]["one"].Value();

            getValue.ShouldThrow<ParseException>()
                    .Which
                    .Message
                    .Should()
                    .Be("An exception occurred while getting the value for option 'one' based on argument(s): not-an-int.");
        }

        public class FileMoveOperation
        {
            public List<FileInfo> Files { get; set; }
            public DirectoryInfo Destination { get; set; }
        }
    }
}