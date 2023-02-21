namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Directive"/>.
    /// </summary>
    public sealed class DirectiveResult : SymbolResult
    {
        internal DirectiveResult(Directive directive, Token token, string? value, SymbolResultTree symbolResultTree, SymbolResult rootCommand)
            : base(symbolResultTree, rootCommand)
        {
            Directive = directive;
            Token = token;
            Value = value;
        }

        /// <summary>
        /// Parsed value of an [name:value] directive.
        /// </summary>
        /// <remarks>Can be null for [name] directives.</remarks>
        public string? Value { get; }

        /// <summary>
        /// The directive to which the result applies.
        /// </summary>
        public Directive Directive { get; }

        /// <summary>
        /// The token that was parsed to specify the directive.
        /// </summary>
        public Token Token { get; }
    }
}
