using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    public class SpanVisitor
    {
        public void Visit(Span span)
        {
            Start(span);

            VisitInternal(span);

            Stop(span);
        }

        private void VisitInternal(Span span)
        {
            switch (span)
            {
                case ContentSpan contentSpan:
                    VisitContentSpan(contentSpan);
                    break;

                case FormatSpan ansiCode:
                    VisitFormatSpan(ansiCode);
                    break;

                case ContainerSpan containerSpan:
                    VisitContainerSpan(containerSpan);
                    break;

                default:
                    VisitUnknownSpan(span);
                    break;
            }
        }

        protected virtual void Start(Span span)
        {
        }

        protected virtual void Stop(Span span)
        {
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
                VisitInternal(span);
            }
        }

        public virtual void VisitContentSpan(ContentSpan contentSpan)
        {
            RecordVisit(contentSpan);
        }

        public virtual void VisitFormatSpan(FormatSpan controlCode)
        {
            RecordVisit(controlCode);
        }

        public IList<Span> VisitedSpans { get; } = new List<Span>();
    }
}
