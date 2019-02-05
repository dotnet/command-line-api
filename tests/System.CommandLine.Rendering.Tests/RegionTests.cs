// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class RegionTests
    {
        [Fact]
        public void Regions_cannot_have_negative_left()
        {
            Action ctorCall = () =>
                new Region(-1, 10, 10, 10);

            ctorCall.Should().Throw<ArgumentOutOfRangeException>()
                    .Which
                    .Message
                    .Should()
                    .Contain("left");
        }

        [Fact]
        public void Regions_cannot_have_negative_top()
        {
            Action ctorCall = () =>
                new Region(10, -1, 10, 10);

            ctorCall.Should().Throw<ArgumentOutOfRangeException>()
                    .Which
                    .Message
                    .Should()
                    .Contain("top");
        }

        [Fact]
        public void Regions_cannot_have_negative_width()
        {
            Action ctorCall = () =>
                new Region(10, 10, -1, 10);

            ctorCall.Should().Throw<ArgumentOutOfRangeException>()
                    .Which
                    .Message
                    .Should()
                    .Contain("width");
        }

        [Fact]
        public void Regions_cannot_have_negative_height()
        {
            Action ctorCall = () =>
                new Region(10, 10, 10, -1);

            ctorCall.Should().Throw<ArgumentOutOfRangeException>()
                    .Which
                    .Message
                    .Should()
                    .Contain("height");
        }
    }
}
