namespace System.CommandLine
{
    public partial class CommandLineConfiguration
    {
        public class SymbolCannotContainDelimiterException : ArgumentException
        {
            public SymbolCannotContainDelimiterException(char delimiter)

            {
                Delimiter = delimiter;
            }

            public char Delimiter { get; }

            public override string Message => $"Symbol cannot contain delimiter: \"{Delimiter}\"";
        }
    }
}
