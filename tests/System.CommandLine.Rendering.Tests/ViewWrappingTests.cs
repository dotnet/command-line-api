// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Linq;
using Xunit;
using System.CommandLine.Rendering.Views;

namespace System.CommandLine.Rendering.Tests
{
    public class ViewWrappingTests
    {
        private readonly TestTerminal _terminal;
        private readonly ConsoleRenderer consoleRenderer;

        public ViewWrappingTests()
        {
            _terminal = new TestTerminal
                       {
                           Width = 150
                       };

            consoleRenderer = new ConsoleRenderer(_terminal);
        }

        [Fact]
        public void Text_wraps_within_the_specified_region()
        {
            var text = "1 1 1 2 2";

            var view = new ContentView(text);

            view.Render(consoleRenderer,
                new Region(0, 0, 5, 2));

            _terminal.RenderOperations()
                    .Select(l => l.Text)
                    .Should()
                    .BeEquivalentTo(
                        new[] {
                            "1 1 1",
                            "2 2  "
                        },
                        options => options.WithStrictOrdering());
        }
    }
}
