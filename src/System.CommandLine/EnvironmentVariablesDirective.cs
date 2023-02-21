using System.CommandLine.Invocation;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
    /// </summary>
    public sealed class EnvironmentVariablesDirective : Directive
    {
        public EnvironmentVariablesDirective() : base("env", syncHandler: SyncHandler)
        {
        }

        private static void SyncHandler(InvocationContext context)
        {
            EnvironmentVariablesDirective symbol = (EnvironmentVariablesDirective)context.ParseResult.Symbol;
            string? parsedValues = context.ParseResult.FindResultFor(symbol)!.Value;

            if (parsedValues is not null)
            {
                string[] components = parsedValues.Split(new[] { '=' }, count: 2);
                string variable = components.Length > 0 ? components[0].Trim() : string.Empty;
                if (string.IsNullOrEmpty(variable) || components.Length < 2)
                {
                    return;
                }

                string value = components[1].Trim();
                Environment.SetEnvironmentVariable(variable, value);
            }
        }
    }
}
