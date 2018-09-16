using System.CommandLine.Rendering.Models;

namespace System.CommandLine.Rendering.Spans
{
    public class ContentSpan : Span
    {
        public ContentSpan(string content)
        {
            Content = content ?? "";
        }

        public string Content { get; }

        public override int ContentLength => Content.Length;

        public override string ToString() => Content;

        protected bool Equals(ContentSpan other) => string.Equals(Content, other.Content);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() &&
                   Equals((AnsiControlCode)obj);
        }

        public override int GetHashCode() => Content?.GetHashCode() ?? 0;
    }
}
