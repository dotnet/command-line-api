using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
    /// </summary>
    public sealed class EnvironmentVariablesDirective : Directive
    {
        public EnvironmentVariablesDirective() : base("env")
        {
            SetSynchronousHandler((context, next) =>
            {
                SetEnvVars(context.ParseResult);

                next?.Invoke(context);
            });
            SetAsynchronousHandler((context, next, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled(cancellationToken);
                }

                SetEnvVars(context.ParseResult);

                return next?.InvokeAsync(context, cancellationToken) ?? Task.CompletedTask;
            });
        }

        private void SetEnvVars(ParseResult parseResult)
        {
            DirectiveResult directiveResult = parseResult.FindResultFor(this)!;

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
        }
    }
}
