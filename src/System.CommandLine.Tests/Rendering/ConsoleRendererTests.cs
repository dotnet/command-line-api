using System.CommandLine.Rendering;
using System.Drawing;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Rendering.Ansi;
using static System.Environment;
using System.CommandLine.Rendering.Views;

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
                $"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}",
                _console.GetRegion());

            var output = _console.Out.ToString().TrimEnd();

            _output.WriteLine(output);

            output.Should().Be("normal");
        }

        [Fact]
        public void When_in_NonAnsi_mode_control_codes_within_tables_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi
            );

            new DirectoryView(new DirectoryInfo(Directory.GetCurrentDirectory())).Render(writer, new Region(0, 0, 100, 100));

            _console.Out
                    .ToString()
                    .Should()
                    .NotContain(Esc);
        }

        [Fact]
        public void When_in_File_mode_control_codes_within_FormattableStrings_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.File
            );

            writer.RenderToRegion(
                $"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}",
                _console.GetRegion());

            var output = _console.Out.ToString();

            _output.WriteLine(output);

            output.Should().Be($"normal");
        }

        [Fact]
        public void When_in_File_mode_control_codes_within_tables_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.File
            );

            new DirectoryView(new DirectoryInfo(Directory.GetCurrentDirectory())).Render(writer, new Region(0, 0, 100, 100));

            _console.Out
                    .ToString()
                    .Should()
                    .NotContain(Esc);
        }

        [Fact]
        public void When_in_Ansi_mode_control_codes_within_FormattableStrings_are_rendered()
        {
            var writer = new ConsoleRenderer(
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

        [Theory]
        [InlineData(ZeroThroughThirty, 0, 0, 10, 1)]
        [InlineData(ZeroThroughThirty, 0, 0, 1, 10)]
        [InlineData("one two", 0, 0, 4, 4)]
        [InlineData("", 0, 0, 4, 4)]
        [InlineData(ZeroThroughThirty, 4, 4, 10, 1)]
        [InlineData(ZeroThroughThirty, 4, 4, 1, 10)]
        [InlineData("one two", 4, 4, 4, 4)]
        [InlineData("", 4, 4, 4, 4)]
        public void When_in_NonAnsi_mode_text_fills_and_does_not_go_beyond_the_width_of_the_specified_region(
            string text,
            int left,
            int top,
            int width,
            int height)
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var region = new Region(left,
                                    top,
                                    width,
                                    height);

            writer.RenderToRegion(text, region);

            _console.RenderOperations()
                    .Should()
                    .OnlyContain(line => line.Text.Length == width);
        }

        [Fact]
        public void When_in_NonAnsi_mode_text_following_newline_within_an_unindented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var region = new Region(0, 0, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _console.RenderOperations()
                    .Select(l => l.Text)
                    .Should()
                    .BeEquivalentTo(
                        new[] {
                            $"     ",
                            $"*    "
                        },
                        options => options.WithStrictOrdering());
        }

        [Fact]
        public void When_in_Ansi_mode_text_following_newline_within_an_unindented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.Ansi);

            var region = new Region(0, 0, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"{Ansi.Cursor.Move.ToLocation(left: 1, top: 1).EscapeSequence}     {Ansi.Cursor.Move.ToLocation(left: 1, top: 2).EscapeSequence}*    ");
        }

        [Fact]
        public void When_in_NonAnsi_mode_text_following_newline_within_an_indented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var region = new Region(13, 17, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _console.Events
                    .OfType<TestConsole.CursorPositionChanged>()
                    .Select(e => e.Position)
                    .Should()
                    .BeEquivalentSequenceTo(
                        new Point(13, 17),
                        new Point(13, 18));
        }

        [Fact]
        public void When_in_Ansi_mode_text_following_newline_within_an_indented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.Ansi);

            var region = new Region(5, 13, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _console.Out
                    .ToString()
                    .Should()
                    .Be($"{Ansi.Cursor.Move.ToLocation(left: 6, top: 14).EscapeSequence}     {Ansi.Cursor.Move.ToLocation(left: 6, top: 15).EscapeSequence}*    ");
        }

        [Theory]
        [InlineData(ZeroThroughThirty, 0, 0, 10, 1)]
        [InlineData(ZeroThroughThirty, 0, 0, 1, 10)]
        [InlineData("one two", 0, 0, 4, 4)]
        [InlineData("", 0, 0, 4, 4)]
        [InlineData(ZeroThroughThirty, 4, 4, 10, 1)]
        [InlineData(ZeroThroughThirty, 4, 4, 1, 10)]
        [InlineData("one two", 4, 4, 4, 4)]
        [InlineData("", 4, 4, 4, 4)]
        public void When_in_NonAnsi_mode_text_fills_and_does_not_go_beyond_the_height_of_the_specified_region(
            string text,
            int left,
            int top,
            int width,
            int height)
        {
            var writer = new ConsoleRenderer(
                _console,
                OutputMode.NonAnsi);

            var region = new Region(left,
                                    top,
                                    width,
                                    height);

            writer.RenderToRegion(text, region);

            _output.WriteLine(string.Join(NewLine, _console.RenderOperations()));

            _console.RenderOperations().Should().HaveCount(height);
        }

        private const string ZeroThroughThirty =
            "zero one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty twenty-one twenty-two twenty-three twenty-four twenty-five twenty-six twenty-seven twenty-eight twenty-nine thirty";

        public class DirectoryView : StackLayoutView
        {
            public DirectoryView(DirectoryInfo directory)
            {
                if (directory == null)
                {
                    throw new ArgumentNullException(nameof(directory));
                }

                var formatter = new SpanFormatter();
                formatter.AddFormatter<DateTime>(d => $"{d:d} {ForegroundColorSpan.DarkGray()}{d:t}{ForegroundColorSpan.Reset()} ");


                AddChild(new ContentView(""));
                AddChild(new ContentView(""));

                AddChild(new ContentView($"Directory: {directory.FullName}"));

                AddChild(new ContentView(""));
                AddChild(new ContentView(""));

                var directoryContents = directory.EnumerateFileSystemInfos()
                                                 .OrderBy(f => f is DirectoryInfo
                                                                   ? 0
                                                                   : 1).ToList();

                var tableView = new TableView<FileSystemInfo>();
                tableView.Items = directoryContents;
                tableView.AddColumn(f => f is DirectoryInfo
                                     ? Span($"{ForegroundColorSpan.LightGreen()}{f.Name}{ForegroundColorSpan.Reset()} ")
                                     : Span($"{ForegroundColorSpan.White()}{f.Name}{ForegroundColorSpan.Reset()} "),
                                     new ContentView(formatter.ParseToSpan($"{Ansi.Text.UnderlinedOn}Name{Ansi.Text.UnderlinedOff}")));

                tableView.AddColumn(f => formatter.Format(f.CreationTime), 
                    new ContentView(formatter.ParseToSpan($"{Ansi.Text.UnderlinedOn}Created{Ansi.Text.UnderlinedOff}")));
                tableView.AddColumn(f => formatter.Format(f.LastWriteTime), 
                    new ContentView(formatter.ParseToSpan($"{Ansi.Text.UnderlinedOn}Modified{Ansi.Text.UnderlinedOff}")));

                AddChild(tableView);

                Span Span(FormattableString formatableString)
                {
                    return formatter.ParseToSpan(formatableString);
                }
            }
        }
    }
}
