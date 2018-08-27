namespace System.CommandLine
{
    public class SymbolCannotContainDelimiterArgumentException : ArgumentException
    {
        public SymbolCannotContainDelimiterArgumentException(char delimiter)

        {
            Delimiter = delimiter;
        }

        public char Delimiter { get; }

        public override string Message => $"Symbol cannot contain delimiter: \"{Delimiter}\"";
    }
}
