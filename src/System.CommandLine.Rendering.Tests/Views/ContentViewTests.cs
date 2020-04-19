// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using System.Drawing;
using FluentAssertions;
using Xunit;
using static System.CommandLine.Rendering.TestTerminal;

namespace System.CommandLine.Rendering.Tests.Views
{
    public class ContentViewTests
    {
        private readonly TestTerminal _terminal;
        private readonly ConsoleRenderer _renderer;

        public ContentViewTests()
        {
            _terminal = new TestTerminal();
            _renderer = new ConsoleRenderer(_terminal);
        }

        [Fact]
        public void When_constructing_with_a_string_a_ContentSpan_is_created()
        {
            var contentView = new TestContentView("Four");
            var span = contentView.GetSpan();
            
            span.Should().BeOfType<ContentSpan>().Which.Content.Should().Contain("Four");
        }

        [Fact]
        public void When_constructing_the_span_argument_cannot_be_null()
        {
            TextSpan span = null;
            Action constructView = () => new ContentView(span);
            
            constructView.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Span_is_not_created_by_default()
        {
            var contentView = new TestContentView();
            
            contentView.IsSpanNull.Should().BeTrue();
        }

        [Fact]
        public void Measure_requires_renderer()
        {
            var contentView = new ContentView("Four");
            contentView
                .Invoking(x => x.Measure(null, new Size(0, 0)))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Measure_requires_maxSize()
        {
            var contentView = new ContentView("Four");
            contentView
                .Invoking(x => x.Measure(_renderer, null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Measure_returns_content_string_size()
        {
            var contentView = new ContentView("Four");
            var size = contentView.Measure(_renderer, new Size(10, 1));
            
            size.Height.Should().Be(1);
            size.Width.Should().Be(4);
        }

        [Fact]
        public void Measuring_without_span_is_zero_size()
        {
            var contentView = new TestContentView();
            var size = contentView.Measure(_renderer, new Size(10, 1));
            
            size.Height.Should().Be(0);
            size.Width.Should().Be(0); 
        }

        [Fact]
        public void Render_requires_renderer()
        {
            var contentView = new ContentView("Four");
            contentView
                .Invoking(x => x.Render(null, new Region(0, 0, 4, 1)))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Render_requires_region()
        {
            var contentView = new ContentView("Four");
            contentView
                .Invoking(x => x.Render(_renderer, null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Render_writes_span_in_region()
        {
            var contentView = new ContentView("Four");

            contentView.Render(_renderer, new Region(0, 0, 4, 1));

            _terminal.Events
                    .Should()
                    .BeEquivalentSequenceTo(
                        new CursorPositionChanged(new Point(0, 0)),
                        new ContentWritten("Four"));
        }

        [Fact]
        public void Views_created_from_an_observable_can_be_updated_by_the_observable()
        {
            var observable = new TestObservable();
            var view = ContentView.FromObservable(observable);
            var isViewUpdated = false;
            view.Updated += (s, e) => { isViewUpdated = true; };

            var initialSize = view.Measure(_renderer, new Size(10, 10));
            initialSize.Height.Should().Be(0);
            initialSize.Width.Should().Be(0);

            observable.UpdateViews("Four");
            
            isViewUpdated.Should().BeTrue();
            var updatedSize = view.Measure(_renderer, new Size(10, 10));
            updatedSize.Height.Should().Be(1);
            updatedSize.Width.Should().Be(4);
        }

        [Fact]
        public void View_measures_whitespace()
        {
            var view = new ContentView("   One Two  ");

            Size measuredSize = view.Measure(_renderer, new Size(6, 2));

            measuredSize.Should().BeEquivalentTo(new Size(6, 2));
        }

        [Fact]
        public void View_renders_whitespace()
        {
            var view = new ContentView("   One Two  ");

            view.Render(_renderer, new Region(0, 0, 6, 2));

            _terminal.Events.Should().BeEquivalentSequenceTo(
                new CursorPositionChanged(new Point(0,0)),
                new ContentWritten("   One"),
                new CursorPositionChanged(new Point(0, 1)),
                new ContentWritten("Two   ")
            );
        }

        private class TestContentView : ContentView
        {
            public bool IsSpanNull => Span == null;

            public TextSpan GetSpan() => Span;

            public TestContentView()
            { }

            public TestContentView(string content) : base(content) { }
        }

        private class TestObservable : IObservable<string>
        {
            private readonly List<IObserver<string>> _observers;

            public TestObservable()
            {
                _observers = new List<IObserver<string>>();
            }

            public IDisposable Subscribe(IObserver<string> observer)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
                return new TestDisposable(_observers, observer);
            }

            public void UpdateViews(string value)
            {
                foreach(var observer in _observers)
                {
                    observer.OnNext(value);
                }
            }
        }

        private class TestDisposable : IDisposable
        {
            private readonly List<IObserver<string>> _observers;
            private readonly IObserver<string> _observer;
            
            public TestDisposable(List<IObserver<string>> observers, IObserver<string> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose() 
            {
                if (_observer != null) _observers.Remove(_observer);
            }
        }
    }
}
