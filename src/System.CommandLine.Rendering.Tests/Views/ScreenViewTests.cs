// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests.Views
{
    public class ScreenViewTests
    {
        private readonly TestTerminal _terminal;
        private readonly ConsoleRenderer _renderer;
        private readonly TestSynchronizationContext _synchronizationContext;

        public ScreenViewTests()
        {
            _terminal = new TestTerminal();
            _renderer = new ConsoleRenderer(_terminal);
            _synchronizationContext = new TestSynchronizationContext();
        }

        [Fact]
        public void ScreenView_requires_a_renderer()
        {
            Action nullRenderer = () => new ScreenView(null, _terminal);

            nullRenderer.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "renderer");
        }

        [Fact]
        public void ScreenView_requires_a_console()
        {
            Action nullRenderer = () => new ScreenView(_renderer, null);

            nullRenderer.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "console");
        }

        [Fact]
        public void Render_hides_the_cursor()
        {
            var screen = new ScreenView(_renderer, _terminal, _synchronizationContext);

            screen.Render();

            _terminal.Events
                .Should()
                .BeEquivalentSequenceTo(new TestTerminal.CursorHidden());
        }

        [Fact]
        public void Dispose_shows_the_cursor()
        {
            var screen = new ScreenView(_renderer, _terminal, _synchronizationContext);

            screen.Dispose();

            _terminal.Events
                .Should()
                .BeEquivalentSequenceTo(new TestTerminal.CursorShown());
        }

        [Fact]
        public void Dispose_unregisters_from_updated_event()
        {
            var screen = new ScreenView(_renderer, _terminal, _synchronizationContext);
            var view = new TestView();

            screen.Child = view;

            screen.Dispose();
            view.RaiseUpdated();

            _synchronizationContext.PostInvocationCount.Should().Be(0);
        }

        [Fact]
        public void Rendering_without_a_child_view_should_not_throw()
        {
            var screen = new ScreenView(_renderer, _terminal, _synchronizationContext);

            Action renderAction = () => screen.Render();

            renderAction.Should().NotThrow();
        }

        [Fact]
        public void On_child_updated_the_screen_is_rendered()
        {
            _terminal.Height = 40;
            _terminal.Width = 100;
            _terminal.CursorLeft = _terminal.CursorTop = 20;

            var screen = new ScreenView(_renderer, _terminal, _synchronizationContext);
            var view = new TestView();
            screen.Child = view;

            view.RaiseUpdated();

            _synchronizationContext.InvokePostCallbacks();
            view.RenderedRegions
                .Should()
                .BeEquivalentSequenceTo(new Region(0, 0, 100, 40));
        }

        [Fact]
        public void On_child_updated_the_render_operation_is_synchronized()
        {
            _terminal.Height = 40;
            _terminal.Width = 100;
            _terminal.CursorLeft = _terminal.CursorTop = 20;

            var screen = new ScreenView(_renderer, _terminal, _synchronizationContext);
            var view = new TestView();
            screen.Child = view;

            //Simulate multiple concurrent updates
            view.RaiseUpdated();
            view.RaiseUpdated();
            _synchronizationContext.InvokePostCallbacks();

            _synchronizationContext.PostInvocationCount.Should().Be(1);
            view.RenderedRegions
                .Should()
                .BeEquivalentSequenceTo(new Region(0, 0, 100, 40));
        }

        [Fact]
        public void On_child_updated_while_a_render_operation_is_in_progress_gets_queued()
        {
            _terminal.Height = 40;
            _terminal.Width = 100;
            _terminal.CursorLeft = _terminal.CursorTop = 20;

            var screen = new ScreenView(_renderer, _terminal, _synchronizationContext);
            var view = new TestView();
            void BeforeRenderAction()
            {
                view.BeforeRender = null;
                view.RaiseUpdated();
                view.RaiseUpdated();
            }
            view.BeforeRender = BeforeRenderAction;
            screen.Child = view;

            //Simulate multiple concurrent updates
            view.RaiseUpdated();
            _synchronizationContext.InvokePostCallbacks();

            _synchronizationContext.PostInvocationCount.Should().Be(2);
            view.RenderedRegions
                .Should()
                .BeEquivalentSequenceTo(
                    new Region(0, 0, 100, 40),
                    new Region(0, 0, 100, 40));
        }

        private class TestSynchronizationContext : SynchronizationContext
        {
            private readonly List<Action> _postActions = new();
            public void InvokePostCallbacks()
            {
                while (_postActions.FirstOrDefault() is Action postAction)
                {
                    _postActions.RemoveAt(0);
                    postAction.Invoke();
                }
            }

            public int PostInvocationCount { get; private set; }
            public override void Post(SendOrPostCallback d, object state)
            {
                PostInvocationCount++;
                _postActions.Add(() => d.Invoke(state));
            }
        }

        private class TestView : View
        {
            public Action BeforeRender { get; set; }
            public List<Region> RenderedRegions { get; } = new List<Region>();

            public override void Render(ConsoleRenderer renderer, Region region)
            {
                BeforeRender?.Invoke();
                RenderedRegions.Add(region);
            }

            public override Size Measure(ConsoleRenderer renderer, Size maxSize) => throw new NotImplementedException();

            public void RaiseUpdated() => OnUpdated();
        }
    }
}
