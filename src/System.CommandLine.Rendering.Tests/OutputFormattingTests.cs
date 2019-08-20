// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Rendering.Ansi;

namespace System.CommandLine.Rendering.Tests
{
    public class OutputFormattingTests
    {
        private readonly ITestOutputHelper _output;
        private readonly ITerminal _terminal;
        private readonly ConsoleRenderer _renderer;

        public OutputFormattingTests(ITestOutputHelper output)
        {
            _output = output;

            _terminal = new TestTerminal {
                Width = 150
            };

            _renderer = new ConsoleRenderer(_terminal);
        }

        [Fact]
        public void Output_can_be_formatted_based_on_type_specific_formatters()
        {
            _renderer.Formatter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");
            
            new TimeSpanView(21.Seconds()).Render(_renderer, new Region(0, 0, 10, 1));

            _terminal.Out.ToString().TrimEnd().Should().Be("21 seconds");
        }

        [Fact]
        public void Type_formatters_apply_to_table_cells()
        {
            var view = new ProcessTimesView(Example_TOP.Processes);

            _renderer.Formatter.AddFormatter<TimeSpan>(ts => $"{ts.TotalSeconds} seconds");

            view.Render(_renderer, new Region(0, 0, 200, 50));

            _output.WriteLine(_terminal.Out.ToString());

            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            _terminal.Out.ToString().Should().Contain($"42{decimalSeparator}82 seconds");
        }

        [Fact]
        public void FormattableString_can_contain_format_strings_that_reformat_the_input_value()
        {
            _renderer.Formatter
                     .AddFormatter<DateTime>(d => $"{d:d} {Color.Foreground.DarkGray.EscapeSequence}{d:t}{Color.Foreground.Default.EscapeSequence}");

            var dateTime = DateTime.Parse("8/2/2018 6pm");

            var span = _renderer.Formatter.Format(dateTime);

            span.ToString().Should().Be($"{dateTime:d} {Color.Foreground.DarkGray.EscapeSequence}{dateTime:t}{Color.Foreground.Default.EscapeSequence}");
        }

        private class TimeSpanView : ContentView<TimeSpan>
        {
            public TimeSpanView(TimeSpan value) : base(value)
            {
            }
        }

        private class ProcessTimesView : TableView<ProcessInfo>
        {
            public ProcessTimesView(IEnumerable<ProcessInfo> processes)
            {
                Items = processes.ToList();

                AddColumn(p => p.Command, "COMMAND");
                AddColumn(p => p.ExecutionTime, "TIME");
            }
        }
    }
}
