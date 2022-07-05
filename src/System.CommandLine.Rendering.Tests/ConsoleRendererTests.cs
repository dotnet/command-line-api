// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Rendering.Ansi;
using static System.CommandLine.Rendering.TestTerminal;
using static System.Environment;
using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;

namespace System.CommandLine.Rendering.Tests
{
    public class ConsoleRendererTests
    {
        private readonly TestTerminal _terminal = new();

        private readonly ITestOutputHelper _output;

        public ConsoleRendererTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void When_in_NonAnsi_mode_control_codes_within_FormattableStrings_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _terminal,
                OutputMode.NonAnsi
            );

            writer.RenderToRegion(
                $"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}",
                _terminal.GetRegion());

            var output = _terminal.Out.ToString().TrimEnd();

            _output.WriteLine(output);

            output.Should().Be("normal");
        }

        [Fact]
        public void When_in_NonAnsi_mode_control_codes_within_tables_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _terminal,
                OutputMode.NonAnsi
            );

            new DirectoryView(new DirectoryInfo(Directory.GetCurrentDirectory())).Render(writer, new Region(0, 0, 100, 100));

            _terminal.Out
                    .ToString()
                    .Should()
                    .NotContain(Esc);
        }

        [Fact]
        public void When_in_File_mode_control_codes_within_FormattableStrings_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _terminal,
                OutputMode.PlainText
            );

            writer.RenderToRegion(
                $"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}",
                new Region(0, 0, 6, 1));

            var output = _terminal.Out.ToString();

            _output.WriteLine(output);

            output.Should().Be($"normal");
        }

        [Fact]
        public void When_in_File_mode_control_codes_within_tables_are_not_rendered()
        {
            var writer = new ConsoleRenderer(
                _terminal,
                OutputMode.PlainText
            );

            new DirectoryView(new DirectoryInfo(Directory.GetCurrentDirectory())).Render(writer, new Region(0, 0, 100, 100));

            _terminal.Out
                    .ToString()
                    .Should()
                    .NotContain(Esc);
        }

        [Fact]
        public void When_in_Ansi_mode_control_codes_within_FormattableStrings_are_rendered()
        {
            var writer = new ConsoleRenderer(
                _terminal,
                OutputMode.Ansi
            );

            writer.RenderToRegion(
                $"{Ansi.Color.Foreground.Red}normal{Ansi.Color.Foreground.Default}",
                _terminal.GetRegion());

            _terminal.Out
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
                _terminal,
                OutputMode.NonAnsi);

            var region = new Region(left,
                                    top,
                                    width,
                                    height);

            writer.RenderToRegion(text, region);

            _terminal.RenderOperations()
                    .Should()
                    .OnlyContain(line => line.Text.Length == width);
        }

        [Fact]
        public void When_in_NonAnsi_mode_text_following_newline_within_an_unindented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _terminal,
                OutputMode.NonAnsi);

            var region = new Region(0, 0, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _terminal.RenderOperations()
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
                _terminal,
                OutputMode.Ansi);

            var region = new Region(0, 0, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _terminal.Out
                    .ToString()
                    .Should()
                    .Be($"{Ansi.Cursor.Move.ToLocation(left: 1, top: 1).EscapeSequence}     {Ansi.Cursor.Move.ToLocation(left: 1, top: 2).EscapeSequence}*    ");
        }

        [Fact]
        public void When_in_NonAnsi_mode_text_following_newline_within_an_indented_region_appears_at_the_correct_left_position()
        {
            var writer = new ConsoleRenderer(
                _terminal,
                OutputMode.NonAnsi);

            var region = new Region(13, 17, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _terminal.Events
                    .OfType<CursorPositionChanged>()
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
                _terminal,
                OutputMode.Ansi);

            var region = new Region(5, 13, 5, 2);

            writer.RenderToRegion($"{NewLine}*", region);

            _terminal.Out
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
                _terminal,
                OutputMode.NonAnsi);

            var region = new Region(left,
                                    top,
                                    width,
                                    height);

            writer.RenderToRegion(text, region);

            _output.WriteLine(string.Join(NewLine, _terminal.RenderOperations()));

            _terminal.RenderOperations().Should().HaveCount(height);
        }

        [Fact]
        public void Text_styles_can_be_automatically_reset_after_render_operations_in_ANSI_mode()
        {
            var renderer = new ConsoleRenderer(_terminal, OutputMode.Ansi, true);

            renderer.RenderToRegion("hello", new Region(0, 0, 5, 1));

            _terminal.Events
                    .Should()
                    .BeEquivalentSequenceTo(
                        new CursorPositionChanged(new Point(0, 0)),
                        new ContentWritten("hello"),
                        new AnsiControlCodeWritten(Ansi.Color.Foreground.Default),
                        new TestTerminal.AnsiControlCodeWritten(Ansi.Color.Background.Default));
        }

        [Fact]
        public void Text_styles_can_be_automatically_reset_after_render_operations_in_non_ANSI_mode()
        {
            var renderer = new ConsoleRenderer(_terminal, OutputMode.NonAnsi, true);

            renderer.RenderToRegion("hello", new Region(0, 0, 5, 1));

            _terminal.Events
                    .Should()
                    .BeEquivalentSequenceTo(
                        new TestTerminal.CursorPositionChanged(new Point(0, 0)),
                        new TestTerminal.ContentWritten("hello"),
                        new TestTerminal.ColorReset(),
                        new TestTerminal.BackgroundColorChanged(ConsoleColor.Black),
                        new TestTerminal.ColorReset(),
                        new TestTerminal.ForegroundColorChanged(ConsoleColor.White));
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

                var formatter = new TextSpanFormatter();
                formatter.AddFormatter<DateTime>(d => $"{d:d} {ForegroundColorSpan.DarkGray()}{d:t}");

                Add(new ContentView(""));
                Add(new ContentView(""));

                Add(new ContentView($"Directory: {directory.FullName}"));

                Add(new ContentView(""));
                Add(new ContentView(""));

                var directoryContents = directory.EnumerateFileSystemInfos()
                                                 .OrderBy(f => f is DirectoryInfo
                                                                   ? 0
                                                                   : 1).ToList();

                var tableView = new TableView<FileSystemInfo>();
                tableView.Items = directoryContents;
                tableView.AddColumn(f => f is DirectoryInfo
                                     ? Span($"{ForegroundColorSpan.LightGreen()}{f.Name} ")
                                     : Span($"{ForegroundColorSpan.White()}{f.Name} "),
                                     new ContentView(formatter.ParseToSpan($"{Ansi.Text.UnderlinedOn}Name{Ansi.Text.UnderlinedOff}")));

                tableView.AddColumn(f => formatter.Format(f.CreationTime), 
                    new ContentView(formatter.ParseToSpan($"{Ansi.Text.UnderlinedOn}Created{Ansi.Text.UnderlinedOff}")));
                tableView.AddColumn(f => formatter.Format(f.LastWriteTime), 
                    new ContentView(formatter.ParseToSpan($"{Ansi.Text.UnderlinedOn}Modified{Ansi.Text.UnderlinedOff}")));

                Add(tableView);

                TextSpan Span(FormattableString formattableString)
                {
                    return formatter.ParseToSpan(formattableString);
                }
            }
        }
    }
}
