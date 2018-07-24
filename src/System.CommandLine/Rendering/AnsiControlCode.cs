namespace System.CommandLine.Rendering
{
    public class AnsiControlCode 
    {
        private readonly string sequence;

        public AnsiControlCode(string sequence)
        {
            this.sequence = sequence;
        }

        public override string ToString() => sequence;

        public static implicit operator AnsiControlCode(string sequence)
        {
            return new AnsiControlCode(sequence);
        }
    }
}