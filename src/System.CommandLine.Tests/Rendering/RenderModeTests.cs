using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests.Rendering
{
    public class RenderModeTests
    {
        private readonly ITestOutputHelper output;
        private readonly TestConsole _console;

        public RenderModeTests(ITestOutputHelper output)
        {
            this.output = output;
            _console = new TestConsole();
        }

        [Fact]
        public void Control_codes_are_not_rendered_when_virtual_terminal_is_disabled()
        {
            var writer = new ConsoleWriter(
                _console,
                OutputMode.NonAnsi
            );

            writer.RenderToRegion(
                Ansi.Color.Foreground.Red,
                _console.GetRegion());

            writer.RenderToRegion(
                "normal",
                _console.GetRegion());

            writer.RenderToRegion(
                Ansi.Color.Foreground.Default,
                _console.GetRegion());

            _console.Out
                    .ToString()
                    .TrimEnd()
                    .Should()
                    .Be("normal");
        }

        [Fact(Skip="WIP")]
        public void Control_codes_are_rendered_when_virtual_terminal_is_enabled()
        {
            var writer = new ConsoleWriter(
                _console,
                OutputMode.Ansi
            );

            writer.RenderToRegion(
                Ansi.Color.Foreground.Red,
                _console.GetRegion());

            writer.RenderToRegion(
                "normal",
                _console.GetRegion());

            writer.RenderToRegion(
                Ansi.Color.Foreground.Default,
                _console.GetRegion());

            _console.Out
                    .ToString()
                    .TrimEnd()
                    .Should()
                    .Be($"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}");
        }

        [Fact(Skip = "WIP")]
        public void Control_codes_within_FormattableStrings_are_not_rendered_when_virtual_terminal_is_disabled()
        {
            var writer = new ConsoleWriter(
                _console,
                OutputMode.NonAnsi
            );

            writer.RenderToRegion(
                $"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}",
                _console.GetRegion());

            _console.Out
                    .ToString()
                    .TrimEnd()
                    .Should()
                    .Be("normal");
        }
    }
}
