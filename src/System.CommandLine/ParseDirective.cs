using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[parse]</c> directive, which when specified on the command line will short circuit normal command handling and display a diagram explaining the parse result for the command line input.
    /// </summary>
    public sealed class ParseDirective : Directive
    {
        /// <param name="errorExitCode">If the parse result contains errors, this exit code will be used when the process exits.</param>
        public ParseDirective(int errorExitCode = 1) : base("parse")
            => Action = new ParseDirectiveAction(errorExitCode);

        private sealed class ParseDirectiveAction : CliAction
        {
            private readonly int _errorExitCode;

            internal ParseDirectiveAction(int errorExitCode) => _errorExitCode = errorExitCode;

            public override int Invoke(InvocationContext context)
            {
                var parseResult = context.ParseResult;
                context.ParseResult.Configuration.Out.WriteLine(parseResult.Diagram());
                return parseResult.Errors.Count == 0 ? 0 : _errorExitCode;
            }

            public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
                => cancellationToken.IsCancellationRequested
                    ? Task.FromCanceled<int>(cancellationToken)
                    : Task.FromResult(Invoke(context));
        }
    }
}
