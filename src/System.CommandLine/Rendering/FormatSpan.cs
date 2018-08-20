using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerDisplay("{Name}")]
    public abstract class FormatSpan : Span
    {
        protected FormatSpan(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            Name = name;
        }

        public string Name { get; }

        public override int ContentLength => 0;

        public override string ToString() => "";

        protected bool Equals(FormatSpan other) => string.Equals(Name, other.Name);

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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((FormatSpan)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return GetType().GetHashCode() * 397 ^ Name.GetHashCode();
            }
        }
    }
}
