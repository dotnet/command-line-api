// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class ContainerSpanTests
    {
        [Fact]
        public void Container_span_supports_add_method()
        {
            var span = new ContainerSpan(new ContentSpan("content"));
            span.Add(new ContentSpan(" with child"));

            span.ContentLength.Should().Be("content with child".Length);
        }

        [Fact]
        public void Container_span_supports_add_string_method()
        {
            var span = new ContainerSpan(new ContentSpan("content"));
            span.Add(" with string");

            span.ContentLength.Should().Be("content with string".Length);
        }
    }
}
