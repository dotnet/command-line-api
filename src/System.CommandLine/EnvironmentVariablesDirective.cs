using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
    /// </summary>
    public sealed class EnvironmentVariablesDirective : CliDirective
    {
        private CliAction? _action;

        public EnvironmentVariablesDirective() : base("env")
        {
        }

        /// <inheritdoc />
        public override CliAction? Action
        {
            get => _action ??= new EnvironmentVariablesDirectiveAction(this);
            set => _action = value ?? throw new ArgumentNullException(nameof(value));
        }

        private sealed class EnvironmentVariablesDirectiveAction : CliAction
        {
            private readonly EnvironmentVariablesDirective _directive;

            internal EnvironmentVariablesDirectiveAction(EnvironmentVariablesDirective directive) => _directive = directive;

            public override int Invoke(ParseResult parseResult)
            {
                SetEnvVars(parseResult);

                return parseResult.CommandResult.Command.Action?.Invoke(parseResult) ?? 0;
            }

            public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<int>(cancellationToken);
                }

                SetEnvVars(parseResult);

                return parseResult.CommandResult.Command.Action is not null
                    ? parseResult.CommandResult.Command.Action.InvokeAsync(parseResult, cancellationToken)
                    : Task.FromResult(0);
            }

            private void SetEnvVars(ParseResult parseResult)
            {
                DirectiveResult directiveResult = parseResult.FindResultFor(_directive)!;

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
