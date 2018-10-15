using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering.Views
{
    public class ScreenViewTests
    {
        private readonly TestConsole _console;
        private readonly ConsoleRenderer _renderer;
        private readonly TestSynchronizationContext _synchronizationContext;

        public ScreenViewTests()
        {
            _console = new TestConsole();
            _renderer = new ConsoleRenderer(_console);
            _synchronizationContext = new TestSynchronizationContext();
        }

        [Fact]
        public void Screen_view_requires_a_renderer()
        {
            Action nullRenderer = () => new ScreenView(null);

            nullRenderer.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "renderer");
        }

        [Fact]
        public void Clearing_child_view_unregisters_from_updated_event()
        {
            var screen = new ScreenView(_renderer, _synchronizationContext);
            var view = new TestView();
            
            screen.Child = view;
            screen.Child = null;
            view.RaiseUpdated();

            _synchronizationContext.PostInvocationCount.Should().Be(0);
        }

        [Fact]
        public void Rendering_without_a_child_view_should_not_throw()
        {
            var screen = new ScreenView(_renderer, _synchronizationContext);

            Action renderAction = () => screen.Render();

            renderAction.Should().NotThrow();
        }

        [Fact]
        public void On_child_updated_the_screen_is_rendered()
        {
            _console.Height = 40;
            _console.Width = 100;
            _console.CursorLeft = _console.CursorTop = 20;

            var screen = new ScreenView(_renderer, _synchronizationContext);
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
            _console.Height = 40;
            _console.Width = 100;
            _console.CursorLeft = _console.CursorTop = 20;

            var screen = new ScreenView(_renderer, _synchronizationContext);
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
            _console.Height = 40;
            _console.Width = 100;
            _console.CursorLeft = _console.CursorTop = 20;

            var screen = new ScreenView(_renderer, _synchronizationContext);
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
            private readonly List<Action> _postActions = new List<Action>();
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
