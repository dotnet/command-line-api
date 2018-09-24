namespace System.CommandLine.Rendering
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
                   Equals((ContentSpan)obj);
        }

        public override int GetHashCode() => Content?.GetHashCode() ?? 0;
    }
}
