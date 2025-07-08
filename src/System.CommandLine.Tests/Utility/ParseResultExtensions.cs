using System.CommandLine.Invocation;
using System.IO;

namespace System.CommandLine.Tests
{
    internal static class ParseResultExtensions
    {
        internal static string Diagram(this ParseResult parseResult)
        {
            TextWriter outputBefore = parseResult.InvocationConfiguration.Output;

            try
            {
                parseResult.InvocationConfiguration.Output = new StringWriter();
                ((SynchronousCommandLineAction)new DiagramDirective().Action!).Invoke(parseResult);
                return parseResult.InvocationConfiguration.Output.ToString()
                    .TrimEnd(); // the directive adds a new line, tests that used to rely on Diagram extension method don't expect it
            }
            finally
            {
                // some of the tests check the Output after getting the Diagram
                parseResult.InvocationConfiguration.Output = outputBefore;
            }
        }
    }
}
