// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class SetHandlerTests
    {
        [Fact]
        public void Value_Descriptor_can_bind_well_known_injected_types()
        {
            var optionX = new Option<string>("-x");
            var optionY = new Option<int>("-y");
            var command = new RootCommand
            {
                optionX,
                optionY
            };

            string boundX = null;
            int boundY = 0;
            ParseResult boundParseResult = null;

            command.SetHandler(
                (string x, int y, ParseResult parseResult) =>
                {
                    boundX = x;
                    boundY = y;
                    boundParseResult = parseResult;
                }, optionX, optionY);

            command.Invoke("-x hello -y 123");

            boundX.Should().Be("hello");
            boundY.Should().Be(123);
            boundParseResult.Should().NotBeNull();
        }
    }
}