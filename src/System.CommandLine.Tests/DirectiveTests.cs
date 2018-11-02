// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Xunit;

namespace System.CommandLine.Tests
{
    public class DirectiveTests
    {
        [Fact]
        public void Directives_should_not_be_considered_as_unmatched_tokens()
        {
            var parser = new CommandLineBuilder()
                .UseParseDirective()
                .AddOption("-y")
                .Build();

            var result = parser.Parse($"{CommandLineBuilder.ExeName} [parse] -y");

            result.UnmatchedTokens.Should().BeEmpty();
        }

        [Fact]
        public void Raw_tokens_still_hold_directives()
        {
            var parser = new CommandLineBuilder()
                .UseParseDirective()
                .AddOption("-y")
                .Build();

            var result = parser.Parse("[parse] -y");

            result.Directives.Should().Contain("parse"); 
            result.Tokens.Should().Contain("[parse]");
        }

        [Fact]
        public void Directives_should_parse_into_the_directives_collection()
        {
            var parser = new CommandLineBuilder()
                .UseParseDirective()
                .AddOption("-y")
                .Build();

            var result = parser.Parse("[parse] -y");

            result.Directives.Should().Contain("parse");
        }

        [Fact]
        public void Multiple_directives_are_allowed()
        {
            var parser = new CommandLineBuilder()
                .UseParseDirective()
                .AddOption("-y")
                .Build();

            var result = parser.Parse("[parse] [suggest] -y");

            result.Directives.Should().Contain("parse");
            result.Directives.Should().Contain("suggest"); 
        }

        [Fact]
        public void Directives_must_be_the_first_argument()
        {
            var parser = new CommandLineBuilder()
                .UseParseDirective()
                .AddOption("-y")
                .Build();

            var result = parser.Parse("-y [suggest]");

            result.UnmatchedTokens.Should().Contain("[suggest]"); 
        }
    }
}
