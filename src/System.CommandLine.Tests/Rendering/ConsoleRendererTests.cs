using System.CommandLine.Rendering;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Rendering.Ansi;
using static System.Environment;

namespace System.CommandLine.Tests.Rendering
{
    public class ConsoleRendererTests
    {
        private readonly TestConsole _console = new TestConsole();
        private readonly ITestOutputHelper _output;

        public ConsoleRendererTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void When_in_NonAnsi_mode_control_codes_within_FormattableStrings_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi
            );

            writer.RenderToRegion(
                $"{Color.Foreground.Red}normal{Color.Foreground.Default}",
                _console.GetRegion());

            _console.Out
                    .ToString()
                    .TrimEnd()
                    .Should()
                    .Be("normal");
        }

        [Fact]
        public void When_in_NonAnsi_mode_control_codes_within_tables_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi
            );

            new DirectoryView(writer).Render(new DirectoryInfo(Directory.GetCurrentDirectory()));

            _console.Out
                    .ToString()
                    .Should()
                    .NotContain(Esc.ToString());
        }

        [Fact]
        public void When_in_Ansi_mode_control_codes_within_FormattableStrings_are_rendered()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.Ansi
            );

            writer.RenderToRegion(
                $"{Color.Foreground.Red}normal{Color.Foreground.Default}",
                _console.GetRegion());

            _console.Out
                    .ToString()
                    .TrimEnd()
                    .Should()
                    .Contain($"{Color.Foreground.Red}normal{Color.Foreground.Default}");
        }

        [Theory]
        [InlineData(ZeroThroughThirty, 10, 1, 0, 0)]
        [InlineData(ZeroThroughThirty, 1, 10, 0, 0)]
        [InlineData("one two", 4, 4, 0, 0)] 
        [InlineData("", 4, 4, 0, 0)] 
        [InlineData(ZeroThroughThirty, 4, 4, 4, 4, Skip = "Issue #168")] // TODO: (When_in_NonAnsi_mode_text_fills_and_does_not_go_beyond_the_width_of_the_specified_region) 
        public void When_in_NonAnsi_mode_text_fills_and_does_not_go_beyond_the_width_of_the_specified_region(
            string text,
            int height,
            int width,
            int top,
            int left)
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var region = new Region(height,
                                    width,
                                    top,
                                    left);

            writer.RenderToRegion(text, region);

            _output.WriteLine(_console.Out.ToString());

            var lines = _console.Out.ToString().Split(NewLine);

            var expectedWidth = width + left;

            lines.Should().OnlyContain(line => line.Length == expectedWidth);
        }

        [Fact]
        public void When_in_NonAnsi_mode_text_following_newline_within_an_unindented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var region = new Region(2, 5, 0, 0);

            writer.RenderToRegion($"{NewLine}*", region);

            _console.Out.ToString().Should().Be($"     {NewLine}*    ");
        }

        [Fact(Skip="WIP")]
        public void When_in_Ansi_mode_text_following_newline_within_an_unindented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.Ansi);

            var region = new Region(2, 5, 0, 0);

            writer.RenderToRegion($"{NewLine}*", region);

            _console.Out.ToString().Should().Be($"     {Cursor.Move.ToLocation(2, 1)}*    ");
        }

        [Fact(Skip="WIP")]
        public void When_in_NonAnsi_mode_text_following_newline_within_an_indented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            throw new NotImplementedException();
        }

        [Fact(Skip="WIP")]
        public void When_in_Ansi_mode_text_following_newline_within_an_indented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.Ansi);

            throw new NotImplementedException();
        }

        [Theory]
        [InlineData(ZeroThroughThirty, 10, 1, 0, 0)]
        [InlineData(ZeroThroughThirty, 1, 10, 0, 0)]
        [InlineData("one two", 4, 4, 0, 0)] 
        [InlineData("", 4, 4, 0, 0)] 
        [InlineData(ZeroThroughThirty, 4, 4, 4, 4, Skip = "Issue #168")] // TODO: (When_in_NonAnsi_mode_text_fills_and_does_not_go_beyond_the_height_of_the_specified_region) 
        public void When_in_NonAnsi_mode_text_fills_and_does_not_go_beyond_the_height_of_the_specified_region(
            string text,
            int height,
            int width,
            int top,
            int left)
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var region = new Region(height,
                                    width,
                                    top,
                                    left);

            writer.RenderToRegion(text, region);

            _output.WriteLine(_console.Out.ToString());

            var lines = _console.Out.ToString().Split(NewLine);

            lines.Length.Should().Be(height);
        }

        [Fact(Skip = "WIP")]
        public void When_in_Ansi_mode_text_fills_and_does_not_go_beyond_the_height_of_the_specified_region()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "WIP")]
        public void When_in_Ansi_mode_text_fills_and_does_not_go_beyond_the_width_of_the_specified_region()
        {
            throw new NotImplementedException();
        }

        private const string ZeroThroughThirty =
            "zero one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty twenty-one twenty-two twenty-three twenty-four twenty-five twenty-six twenty-seven twenty-eight twenty-nine thirty";

        public class DirectoryView : ConsoleView<DirectoryInfo>
        {
            public DirectoryView(ConsoleRenderer renderer, Region region = null) : base(renderer, region)
            {
                renderer.Formatter
                        .AddFormatter<DateTime>(d => $"{d:d} {Color.Foreground.DarkGray}{d:t}{Color.Foreground.Default}");
            }

            public override void Render(DirectoryInfo directory)
            {
                WriteLine();
                WriteLine();

                Write($"Directory: {directory.FullName}");

                WriteLine();
                WriteLine();

                var directoryContents = directory.EnumerateFileSystemInfos()
                                                 .OrderBy(f => f is DirectoryInfo
                                                                   ? 0
                                                                   : 1);

                RenderTable(
                    directoryContents,
                    table => {
                        table.RenderColumn(
                            Span($"{Ansi.Text.UnderlinedOn}Name{Ansi.Text.UnderlinedOff}"),
                            f =>
                                f is DirectoryInfo
                                    ? Span($"{Color.Foreground.LightGreen}{f.Name}{Color.Foreground.Default}")
                                    : Span($"{Color.Foreground.White}{f.Name}{Color.Foreground.Default}"));

                        table.RenderColumn(
                            Span($"{Ansi.Text.UnderlinedOn}Created{Ansi.Text.UnderlinedOff}"),
                            f => f.CreationTime);

                        table.RenderColumn(
                            Span($"{Ansi.Text.UnderlinedOn}Modified{Ansi.Text.UnderlinedOff}"),
                            f => f.LastWriteTime);
                    }
                );
            }
        }
    }
}
