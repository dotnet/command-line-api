using System.CommandLine.Rendering;
using FluentAssertions;
using System.Linq;
using Xunit;
using static System.CommandLine.Rendering.Ansi;

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
                Cursor.SavePosition,
                new ContainerSpan(
                    Color.Foreground.Red,
                    new ContentSpan("the content"),
                    Color.Foreground.Default),
                Cursor.RestorePosition);

            var visitor = new SpanVisitor();

            visitor.Visit(outerContainer);

            visitor.VisitedSpans
                   .Select(s => s.GetType())
                   .Should()
                   .BeEquivalentTo(
                       expectation: new[] {
                           typeof(ContainerSpan),
                           typeof(AnsiControlCode),
                           typeof(ContainerSpan),
                           typeof(AnsiControlCode),
                           typeof(ContentSpan),
                           typeof(AnsiControlCode),
                           typeof(AnsiControlCode),
                       },
                       config: options => options.WithStrictOrdering()
                   );
        }
    }
}
