using System.CommandLine.Rendering;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class RenderModeTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public void Control_codes_within_FormattableStrings_are_not_rendered_when_ansi_mode_is_disabled()
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

        [Fact]
        public void Control_codes_within_FormattableStrings_are_rendered_when_ansi_mode_is_enabled()
        {
            var writer = new ConsoleWriter(
                _console,
                OutputMode.Ansi
            );

            writer.RenderToRegion(
                $"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}",
                _console.GetRegion());

            _console.Out
                    .ToString()
                    .TrimEnd()
                    .Should()
                    .Contain($"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}");
        }
    }
}
