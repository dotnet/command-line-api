// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ParseDiagramTests
    {
        [Fact]
        public void Parse_result_diagram_helps_explain_parse_operation()
        {
            var parser = new Parser(
                new CommandDefinition("the-command", "Does the thing.", new[] { new OptionDefinition(
                    "-x",
                    "Specifies value x",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()), (SymbolDefinition) new OptionDefinition(
                    "-y",
                    "Specifies value y",
                    argumentDefinition: ArgumentDefinition.None) }, new ArgumentDefinitionBuilder().ZeroOrMore()));

            var result = parser.Parse("the-command -x one -y two three");

            result.Diagram()
                  .Should()
                  .Be("[ the-command [ -x <one> ] [ -y ] <two> <three> ]");
        }

        [Fact]
        public void Parse_result_diagram_helps_explain_partial_parse_operation()
        {
            var parser = new Parser(
                new CommandDefinition("command", "", new[] {
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder()
                                            .FromAmong("arg1", "arg2", "arg3")
                                            .ExactlyOne())
                }));

            var result = parser.Parse("command -x ar");

            result.Diagram()
                  .Should()
                  .Be("[ command [ -x ] ]   ???--> ar");
        }
    }
}
