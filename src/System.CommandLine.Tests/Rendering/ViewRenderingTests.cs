using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Models;
using System.Drawing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class ViewRenderingTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact(Skip = "WIP")]
        public void In_NonAnsi_mode_ConsoleView_keeps_track_of_position_so_that_multiple_WriteLine_statements_do_not_overwrite_the_target_region()
        {
            var renderer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var view = new StringsView(renderer, new CommandLine.Rendering.Region(3, 5, 1, 3));

            view.Render(new[] {
                "1",
                "2",
                "3"
            });

            _console.RenderOperations()
                    .Should()
                    .BeEquivalentTo(new[] {
                        new TextRendered("1", new Point(3, 5)),
                        new TextRendered("2", new Point(3, 6)),
                        new TextRendered("3", new Point(3, 7)),
                    }, options => options.WithStrictOrdering());
        }

        [Fact(Skip = "WIP")]
        public void In_Ansi_mode_ConsoleView_keeps_track_of_position_so_that_multiple_WriteLine_statements_do_not_overwrite_the_target_region()
        {
            var renderer = new ConsoleRenderer(
                _console,
                OutputMode.Ansi);

            var view = new StringsView(renderer, new CommandLine.Rendering.Region(3, 5, 1, 3));

            view.Render(new[] {
                "1",
                "2",
                "3"
            });

            _console.RenderOperations()
                .Should()
                .BeEquivalentTo(new[] {
                    new TextRendered("1", new Point(3, 5)),
                    new TextRendered("2", new Point(3, 6)),
                    new TextRendered("3", new Point(3, 7)),
                }, options => options.WithStrictOrdering());
        }

        private class StringsView : ConsoleView<IEnumerable<string>>
        {
            public StringsView(ConsoleRenderer renderer, CommandLine.Rendering.Region region = null) : base(renderer, region)
            {
            }

            protected override void OnRender(IEnumerable<string> items)
            {
                foreach (var item in items)
                {
                    WriteLine(item);
                }
            }
        }
    }
}
