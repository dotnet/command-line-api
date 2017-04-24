// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class ParseDiagramTests
    {
        [Fact]
        public void Parse_result_diagram_helps_explain_parse_operation()
        {
            var parser = new Parser(
                Create.Command("the-command",
                               "Does the thing.",
                               Accept.ZeroOrMoreArguments(),
                               Create.Option("-x", "Specifies value x", Accept.ExactlyOneArgument()),
                               Create.Option("-y", "Specifies value y", Accept.NoArguments())));

            var result = parser.Parse("the-command -x one -y two three");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ -x <one> ] [ -y ] <two> <three> ]");
        }

        [Fact]
        public void Parse_result_diagram_helps_explain_partial_parse_operation()
        {
            var parser = new Parser(
                Create.Command("command", "",
                               Create.Option("-x", "",
                                             arguments: Accept.AnyOneOf("arg1", "arg2", "arg3"))));

            var result = parser.Parse("command -x ar");

            result.Diagram()
                  .Should()
                  .Be("[ command [ -x ] ]   ???--> ar");
        }
    }
}
