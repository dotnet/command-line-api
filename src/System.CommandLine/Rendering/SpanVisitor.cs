using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    public class SpanVisitor
    {
        public void Visit(Span span)
        {
            switch (span)
            {
                case ContentSpan contentSpan:
                    VisitContentSpan(contentSpan);
                    break;

                case AnsiControlCode ansiCode:
                    VisitAnsiControlCode(ansiCode);
                    break;

                case ContainerSpan containerSpan:
                    VisitContainerSpan(containerSpan);
                    break;

                default:
                    VisitUnknownSpan(span);
                    break;
            }
        }

        public virtual void VisitUnknownSpan(Span span)
        {
            RecordVisit(span);
        }

        protected void RecordVisit(Span span)
        {
            VisitedSpans.Add(span);
        }

        public virtual void VisitContainerSpan(ContainerSpan containerSpan)
        {
            RecordVisit(containerSpan);

            foreach (var span in containerSpan)
            {
                Visit(span);
            }
        }

        public virtual void VisitContentSpan(ContentSpan contentSpan)
        {
            RecordVisit(contentSpan);
        }

        public virtual void VisitAnsiControlCode(AnsiControlCode controlCode)
        {
            RecordVisit(controlCode);
        }

        public IList<Span> VisitedSpans { get; } = new List<Span>();
    }
}