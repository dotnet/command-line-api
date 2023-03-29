using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[parse]</c> directive, which when specified on the command line will short 
    /// circuit normal command handling and display a diagram explaining the parse result for the command line input.
    /// </summary>
    public sealed class ParseDirective : Directive
    {
        private const int DefaultErrorExitCode = 1;
        private CliAction? _action;

        /// <param name="errorExitCode">If the parse result contains errors, this exit code will be used when the process exits.</param>
        public ParseDirective(int errorExitCode = DefaultErrorExitCode) : base("parse")
        {
            if (errorExitCode != DefaultErrorExitCode)
            {
                Action = new ParseDirectiveAction(errorExitCode);
            }
        }

        /// <inheritdoc />
        public override CliAction? Action
        {
            get => _action ??= new ParseDirectiveAction(DefaultErrorExitCode);
            set => _action = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
