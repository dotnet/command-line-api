﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public abstract class SpanVisitor
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

                case ContainerSpan containerSpan:
                    VisitContainerSpan(containerSpan);
                    break;

                case ForegroundColorSpan foregroundColorSpan:
                    VisitForegroundColorSpan(foregroundColorSpan);
                    break;

                case BackgroundColorSpan backgroundColorSpan:
                    VisitBackgroundColorSpan(backgroundColorSpan);
                    break;

                case StyleSpan styleSpan:
                    VisitStyleSpan(styleSpan);
                    break;

                case CursorControlSpan cursorControlSpan:
                    VisitCursorControlSpan(cursorControlSpan);
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
        }

        public virtual void VisitContainerSpan(ContainerSpan containerSpan)
        {
            foreach (var span in containerSpan)
            {
                VisitInternal(span);
            }
        }

        public virtual void VisitContentSpan(ContentSpan contentSpan)
        {
        }

        public virtual void VisitForegroundColorSpan(ForegroundColorSpan span)
        {
        }

        public virtual void VisitBackgroundColorSpan(BackgroundColorSpan span)
        {
        }

        public virtual void VisitStyleSpan(StyleSpan span)
        {
        }

        public virtual void VisitCursorControlSpan(CursorControlSpan cursorControlSpan)
        {
        }
    }
}
