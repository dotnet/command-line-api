// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class MultipleArguments
        {
            private readonly ITestOutputHelper _output;

            public MultipleArguments(ITestOutputHelper output)
            {
                _output = output;
            }

            [Fact(Skip = "wip")]
            public void Multiple_arguments_can_differ_by_arity()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>
                    {
                        Arity = new ArgumentArity(3, 3),
                        Name = "several"
                    },
                    new Argument<string>
                    {
                        Arity = ArgumentArity.ZeroOrMore,
                        Name = "one"
                    }
                };

                var result = command.Parse("1 2 3 4");

                _output.WriteLine(result.ToString());

                var several = result.CommandResult
                                    .GetArgumentValueOrDefault<IEnumerable<string>>("several");

                var one = result.CommandResult
                                .GetArgumentValueOrDefault<string>("one");

                several.Should()
                       .BeEquivalentTo("1", "2", "3");
                one.Should()
                   .Be("4");
            }

            [Fact(Skip = "wip")]
            public void Multiple_arguments_can_differ_by_type()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>
                    {
                        Name = "several"
                    },
                    new Argument<int>
                    {
                        Name = "one"
                    }
                };

                var result = command.Parse("1 2");

                _output.WriteLine(result.ToString());

                Assert.True(false, "Test Multiple_positional_arguments_can_differ_by_type is not written yet.");
            }
        }
    }
}
