namespace System.CommandLine.Tests
{
    internal static class ParseResultExtensions
    {
        internal static string Diagram(this ParseResult parseResult)
            => parseResult.ToString().TrimEnd(); // the directive adds a new line, tests that used to rely on Diagram extension method don't expect it
    }
}
