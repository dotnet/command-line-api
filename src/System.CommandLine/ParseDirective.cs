using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;

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
            SetHandler(SyncHandler);
            ErrorExitCode = errorExitCode;
        }

        internal int ErrorExitCode { get; }

        private int SyncHandler(InvocationContext context)
        {
            var parseResult = context.ParseResult;
            context.Console.Out.WriteLine(parseResult.Diagram());
            return parseResult.Errors.Count == 0 ? 0 : ErrorExitCode;
        }
    }
}
