using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
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
        {
            ErrorExitCode = errorExitCode;

            SetSynchronousHandler(PrintDiagramAndQuit);
            SetAsynchronousHandler((context, next, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled(cancellationToken);
                }

                PrintDiagramAndQuit(context, null);

                return Task.CompletedTask;
            });
        }

        internal int ErrorExitCode { get; }

        private void PrintDiagramAndQuit(InvocationContext context, ICommandHandler? next)
        {
            var parseResult = context.ParseResult;
            context.Console.Out.WriteLine(parseResult.Diagram());
            context.ExitCode = parseResult.Errors.Count == 0 ? 0 : ErrorExitCode;

            // parse directive has a precedence over --help and --version and any command
            // we don't invoke next here.
        }
    }
}
