using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace System.CommandLine.Rendering.Tests
{
    public class HelpBuilderTests : CommandLine.Tests.Help.HelpBuilderTests
    {
        private readonly ConsoleRenderer _renderer;

        public HelpBuilderTests(ITestOutputHelper output) : base(output, new TestTerminal())
        {
            _renderer = new ConsoleRenderer(_console);
        }

        protected override IHelpBuilder GetHelpBuilder(int maxWidth)
        {
            ((TestTerminal)_console).Width = maxWidth;
            return new System.CommandLine.Rendering.Help.HelpBuilder(
                console: _console,
                renderer: _renderer
            );
        }
    }
}
