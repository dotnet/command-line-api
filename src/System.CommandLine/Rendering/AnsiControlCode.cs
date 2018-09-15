using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerStepThrough]
    public class AnsiControlCode
    {
        public string EscapeSequence { get; }

        public AnsiControlCode(string escapeSequence)
        {
            if (string.IsNullOrWhiteSpace(escapeSequence))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(escapeSequence));
            }

            EscapeSequence = escapeSequence;
        }

        public override string ToString() => "";

        protected bool Equals(AnsiControlCode other) => string.Equals(EscapeSequence, other.EscapeSequence);

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

        public override int GetHashCode() => EscapeSequence.GetHashCode();

        public static implicit operator AnsiControlCode(string sequence)
        {
            return new AnsiControlCode(sequence);
        }
    }
}
