// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using System.Drawing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests.Views
{
    public class ContentViewGenericTests
    {
        private readonly TestTerminal _terminal;
        private readonly ConsoleRenderer _renderer;

        public ContentViewGenericTests()
        {
            _terminal = new TestTerminal();
            _renderer = new ConsoleRenderer(_terminal);
        }

        [Fact]
        public void Measure_creates_formatted_span()
        {
            var view = new ContentView<int>(421);

            Size measuredSize = view.Measure(_renderer, new Size(10, 1));
            measuredSize.Width.Should().Be(3);
            measuredSize.Height.Should().Be(1);
        }

        [Fact]
        public void Render_outputs_formatted_span()
        {
            var view = new ContentView<int>(421);

            view.Render(_renderer, new Region(0, 0, 3, 1));

            _terminal.Events
                    .Should()
                    .BeEquivalentSequenceTo(
                        new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                        new TestTerminal.ContentWritten("421"));
        }

        [Fact]
        public void Span_is_only_created_after_measure_is_called()
        {
            var view = new TestableContentView<int>(42);

            view.IsSpanCreated.Should().BeFalse();
            view.Measure(_renderer, new Size(0, 0));
            view.IsSpanCreated.Should().BeTrue();
        }

        [Fact]
        public void Span_is_only_created_after_render_is_called()
        {
            var view = new TestableContentView<int>(42);

            view.IsSpanCreated.Should().BeFalse();
            view.Render(_renderer, new Region(0, 0, 0, 0));
            view.IsSpanCreated.Should().BeTrue();
        }

        [Fact]
        public void Span_is_only_created_once_on_calls_to_render()
        {
            var view = new TestableContentView<int>(42);

            view.Render(_renderer, new Region(0, 0, 0, 0));
            TextSpan firstSpan = view.GetSpan();
            view.Render(_renderer, new Region(0, 0, 0, 0));

            ReferenceEquals(view.GetSpan(), firstSpan).Should().BeTrue();
        }

        [Fact]
        public void Span_is_only_created_once_on_calls_to_measure()
        {
            var view = new TestableContentView<int>(42);

            view.Measure(_renderer, new Size(0, 0));
            TextSpan firstSpan = view.GetSpan();
            view.Measure(_renderer, new Size(0, 0));

            ReferenceEquals(view.GetSpan(), firstSpan).Should().BeTrue();
        }

        private class TestableContentView<T> : ContentView<T>
        {
            public bool IsSpanCreated => Span != null;

            public TextSpan GetSpan() => Span;

            public TestableContentView(T value)
                : base(value)
            {
            }
        }
    }
}
