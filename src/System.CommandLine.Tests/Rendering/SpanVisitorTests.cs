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
        public void SpanVisitor_visits_child_spans_in_depth_first_order()
        {
            var outerContainer = new ContainerSpan(
                Cursor.SavePositionAndAttributes,
                new ContainerSpan(
                    Color.Foreground.Red,
                    new ContentSpan("the content"),
                    Color.Foreground.Default),
                Cursor.RestorePositionAndAttributes);

            var visitor = new SpanVisitor();

            visitor.Visit(outerContainer);

            visitor.VisitedSpans
                   .Select(s => s.ToString())
                   .Should()
                   .BeEquivalentTo(
                       expectation: new[] {
                           typeof(ContainerSpan).ToString(),
                           Cursor.SavePositionAndAttributes.ToString(),
                           typeof(ContainerSpan).ToString(),
                           Color.Foreground.Red.ToString(),
                           "the content",
                           Color.Foreground.Default.ToString(),
                           Cursor.RestorePositionAndAttributes.ToString()
                       },
                       config: options => options.WithStrictOrdering()
                   );
        }
    }
}
