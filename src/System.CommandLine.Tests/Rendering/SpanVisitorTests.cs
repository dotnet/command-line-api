using System.Collections.Generic;
using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests.Rendering
{
    public class SpanVisitorTests
    {
        [Fact]
        public void Initialize_is_only_called_once()
        {
            var span = new ContainerSpan(
                new ContainerSpan(
                    new ContainerSpan()),
                new ContentSpan("hello")
            );

            var visitor = new TestVisitor();

            visitor.Visit(span);

            visitor.InitializeCount.Should().Be(1);
        }

        public class TestVisitor : SpanVisitor
        {
            public int InitializeCount { get; set; }

            protected override void Start(Span span)
            {
                InitializeCount++;
            }
        }

        [Fact]
        public void SpanVisitor_visits_child_spans_in_depth_first_order()
        {
            var outerContainer = new ContainerSpan(
                BackgroundColorSpan.Green(),
                new ContainerSpan(
                    ForegroundColorSpan.Red(),
                    new ContentSpan("the content"),
                    ForegroundColorSpan.Reset()),
                BackgroundColorSpan.Reset());

            var visitor = new RecordingSpanVisitor();

            visitor.Visit(outerContainer);

            visitor.VisitedSpans
                   .Select(s => s.GetType())
                   .Should()
                   .BeEquivalentSequenceTo(
                       typeof(ContainerSpan),
                       typeof(BackgroundColorSpan),
                       typeof(ContainerSpan),
                       typeof(ForegroundColorSpan),
                       typeof(ContentSpan),
                       typeof(ForegroundColorSpan),
                       typeof(BackgroundColorSpan));
        }
    }

    public class RecordingSpanVisitor : SpanVisitor
    {
        public override void VisitUnknownSpan(Span span) => VisitedSpans.Add(span);

        public override void VisitContainerSpan(ContainerSpan span)
        {
            VisitedSpans.Add(span);

            base.VisitContainerSpan(span);
        }

        public override void VisitContentSpan(ContentSpan span) => VisitedSpans.Add(span);

        public override void VisitForegroundColorSpan(ForegroundColorSpan span) => VisitedSpans.Add(span);

        public override void VisitBackgroundColorSpan(BackgroundColorSpan span) => VisitedSpans.Add(span);

        public override void VisitStyleSpan(StyleSpan span) => VisitedSpans.Add(span);

        public List<Span> VisitedSpans { get; } = new List<Span>();
    }
}
