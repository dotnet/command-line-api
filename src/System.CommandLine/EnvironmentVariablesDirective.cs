using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
    /// </summary>
    public sealed class EnvironmentVariablesDirective : Directive
    {
        public EnvironmentVariablesDirective() : base("env")
            => Action = new EnvironmentVariablesDirectiveAction(this);

        private sealed class EnvironmentVariablesDirectiveAction : CliAction
        {
            private readonly EnvironmentVariablesDirective _directive;

            internal EnvironmentVariablesDirectiveAction(EnvironmentVariablesDirective directive) => _directive = directive;

            public override int Invoke(InvocationContext context)
            {
                SetEnvVars(context);

                return context.ParseResult.CommandResult.Command.Action?.Invoke(context) ?? 0;
            }

            public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<int>(cancellationToken);
                }

                SetEnvVars(context);

                return context.ParseResult.CommandResult.Command.Action is not null
                    ? context.ParseResult.CommandResult.Command.Action.InvokeAsync(context, cancellationToken)
                    : Task.FromResult(0);
            }

            private void SetEnvVars(InvocationContext context)
            {
                DirectiveResult directiveResult = context.ParseResult.FindResultFor(_directive)!;

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
}
