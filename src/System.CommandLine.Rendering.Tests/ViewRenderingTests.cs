// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests;
using System.Drawing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class ViewRenderingTests
    {
        private readonly TestTerminal _terminal = new TestTerminal();

        [Fact(Skip = "WIP")]
        public void In_NonAnsi_mode_ConsoleView_keeps_track_of_position_so_that_multiple_WriteLine_statements_do_not_overwrite_the_target_region()
        {
            var renderer = new ConsoleRenderer(
                _terminal,
                OutputMode.NonAnsi);

            var view = new StringsView(new[] {
                "1",
                "2",
                "3"
            });

            view.Render(renderer, new Region(3, 5, 1, 3));

            _terminal.RenderOperations()
                    .Should()
                    .BeEquivalentSequenceTo(new TextRendered("1", new Point(3, 5)),
                        new TextRendered("2", new Point(3, 6)),
                        new TextRendered("3", new Point(3, 7)));
        }

        [Fact(Skip = "WIP")]
        public void In_Ansi_mode_ConsoleView_keeps_track_of_position_so_that_multiple_WriteLine_statements_do_not_overwrite_the_target_region()
        {
            var renderer = new ConsoleRenderer(
                _terminal,
                OutputMode.Ansi);
            
            var view = new StringsView(new[] {
                "1",
                "2",
                "3"
            });

            view.Render(renderer, new Region(3, 5, 1, 3));

            _terminal.RenderOperations()
                    .Should()
                    .BeEquivalentSequenceTo(
                        new TextRendered("1", new Point(3, 5)),
                        new TextRendered("2", new Point(3, 6)),
                        new TextRendered("3", new Point(3, 7)));
        }

        private class StringsView : StackLayoutView
        {
            public StringsView(string[] strings)
            {
                foreach(var @string in strings)
                {
                    Add(new ContentView(@string));
                }
            }
        }
    }
}
