using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerStepThrough]
    public class AnsiControlCode : Span
    {
        private readonly string _sequence;

        public AnsiControlCode(string sequence)
        {
            if (string.IsNullOrWhiteSpace(sequence))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(sequence));
            }

            _sequence = sequence;
        }

        public override string ToString() => _sequence;

        public override int ContentLength => 0;

        protected bool Equals(AnsiControlCode other) => string.Equals(_sequence, other._sequence);

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

        public override int GetHashCode() => _sequence.GetHashCode();

        public static implicit operator AnsiControlCode(string sequence)
        {
            return new AnsiControlCode(sequence);
        }
    }
}
