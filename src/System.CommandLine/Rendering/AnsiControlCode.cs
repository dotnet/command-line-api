namespace System.CommandLine.Rendering
{
    public class AnsiControlCode : Span
    {
        private readonly string sequence;

        public AnsiControlCode(string sequence)
        {
            if (string.IsNullOrWhiteSpace(sequence))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(sequence));
            }

            this.sequence = sequence;
        }

        public override string ToString() => sequence;

        public static implicit operator AnsiControlCode(string sequence)
        {
            return new AnsiControlCode(sequence);
        }

        public override int ContentLength => 0;

        protected bool Equals(AnsiControlCode other) => string.Equals(sequence, other.sequence);

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

        public override int GetHashCode() => sequence?.GetHashCode() ?? 0;
    }
}
