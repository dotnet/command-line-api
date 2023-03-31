using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[parse]</c> directive, which when specified on the command line will short 
    /// circuit normal command handling and display a diagram explaining the parse result for the command line input.
    /// </summary>
    public sealed class ParseDiagramDirective : CliDirective
    {
        private CliAction? _action;

        /// <summary>
        /// Writes a diagram of the parse result to the console.
        /// </summary>
        public ParseDiagramDirective() : base("parse")
        {
        }

        /// <inheritdoc />
        public override CliAction? Action
        {
            get => _action ??= new ParseDiagramAction(ParseErrorReturnValue);
            set => _action = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the return value, which can be used as an exit code, when parsing encounters an error.
        /// </summary>
        public int ParseErrorReturnValue { get; set; } = 1;
    }
}