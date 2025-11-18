using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Represents a result produced when parsing a <see cref="Directive"/>.
    /// </summary>
    public sealed class DirectiveResult : SymbolResult
    {
        private List<string>? _values;

        internal DirectiveResult(Directive directive, Token token, SymbolResultTree symbolResultTree)
            : base(symbolResultTree, null) // directives don't belong to any command
        {
            Directive = directive;
            Token = token;
        }

        /// <summary>
        /// Parsed values of <c>[name:value]</c> directives.
        /// </summary>
        /// <remarks>Can be empty for <c>[name]</c> directives.</remarks>
        public IReadOnlyList<string> Values => _values is null ? Array.Empty<string>() : _values;

        /// <summary>
        /// Gets the directive to which the result applies.
        /// </summary>
        public Directive Directive { get; }

        /// <summary>
        /// Gets the token that was parsed to specify the directive.
        /// </summary>
        public Token Token { get; }

        internal void AddValue(string value) => (_values ??= new()).Add(value);
    }
}
