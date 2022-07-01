// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using System.Drawing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class ViewRenderingTests
    {
        private readonly TestTerminal _terminal = new();

        [Fact]
        public void Views_can_be_registered_for_specific_types()
        {
            ParseResult parseResult = null;

            var command = new RootCommand
            {
                Handler = CommandHandler.Create<ParseResult, IConsole>(
                    (r, c) =>
                    {
                        parseResult = r;
                        c.Append(new ParseResultView(r));
                    })
            };

            var parser = new CommandLineBuilder(command)
                         .AddMiddleware(c =>
                         {
                             c.BindingContext
                              .AddService(
                                  s => new ParseResultView(s.GetService<ParseResult>()));
                         })
                         .Build();

            var terminal = new TestTerminal
            {
                IsAnsiTerminal = false
            };

            parser.Invoke("", terminal);

            terminal.Out.ToString().Should().Contain(parseResult.Diagram());
        }

        [Theory]
        [InlineData(OutputMode.NonAnsi)]
        [InlineData(OutputMode.Ansi)]
        public void Views_can_be_appended_to_output(OutputMode outputMode)
        {
            var view = new StringsView(new[]
            {
                "1",
                "2",
                "3"
            });

            _terminal.Append(view, outputMode);

            _terminal.RenderOperations()
                     .Should()
                     .BeEquivalentSequenceTo(
                         new TextRendered("1", new Point(0, 0)),
                         new TextRendered("2", new Point(0, 1)),
                         new TextRendered("3" + Environment.NewLine, new Point(0, 2)));
        }

        [Theory]
        [InlineData(OutputMode.NonAnsi)]
        [InlineData(OutputMode.Ansi)]
        public void ConsoleView_keeps_track_of_position_so_that_multiple_WriteLine_statements_do_not_overwrite_the_target_region(OutputMode outputMode)
        {
            var renderer = new ConsoleRenderer(
                _terminal,
                outputMode);

            var view = new StringsView(new[]
            {
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
                foreach (var @string in strings)
                {
                    Add(new ContentView(@string));
                }
            }
        }
    }

    public class ParseResultView : ContentView<ParseResult>
    {
        public ParseResultView(ParseResult value) : base(value)
        {
        }
    }
}