// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.Define;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class ParseDiagramTests
    {
        [Fact]
        public void Parse_result_diagram_helps_explain_parse_operation()
        {
            var parser = new CommandParser(
                Create.Command("the-command",
                               "Does the thing.",
                               new ArgumentRuleBuilder().ZeroOrMore(),
                               Create.Option("-x", "Specifies value x", new ArgumentRuleBuilder().ExactlyOne()),
                               Create.Option("-y", "Specifies value y", ArgumentsRule.None)));

            var result = parser.Parse("the-command -x one -y two three");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ -x <one> ] [ -y ] <two> <three> ]");
        }

        [Fact]
        public void Parse_result_diagram_helps_explain_partial_parse_operation()
        {
            var parser = new CommandParser(
                Create.Command("command", "",
                               Create.Option("-x", "",
                                             arguments: new ArgumentRuleBuilder().FromAmong(new[] {"arg1", "arg2", "arg3"}).ExactlyOne())));

            var result = parser.Parse("command -x ar");

            result.Diagram()
                  .Should()
                  .Be("[ command [ -x ] ]   ???--> ar");
        }
    }
}
