using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
    /// </summary>
    public sealed class EnvironmentVariablesDirective : Directive
    {
        public EnvironmentVariablesDirective() : base("env")
        {
            SetHandler(SyncHandler);
        }

        private int SyncHandler(InvocationContext context)
        {
            DirectiveResult directiveResult = context.ParseResult.FindResultFor(this)!;

            for (int i = 0; i < directiveResult.Values.Count; i++)
            {
                string parsedValue = directiveResult.Values[i];

                int indexOfSeparator = parsedValue.AsSpan().IndexOf('=');
                
                if (indexOfSeparator > 0)
                {
                    ReadOnlySpan<char> variable = parsedValue.AsSpan(0, indexOfSeparator).Trim();

                    if (!variable.IsEmpty)
                    {
                        string value = parsedValue.AsSpan(indexOfSeparator + 1).Trim().ToString();

                        Environment.SetEnvironmentVariable(variable.ToString(), value);
                    }
                }
            }

            // we need a cleaner, more flexible and intuitive way of continuing the execution
            return context.ParseResult.CommandResult.Command.Handler?.Invoke(context) ?? 0;
        }
    }
}
