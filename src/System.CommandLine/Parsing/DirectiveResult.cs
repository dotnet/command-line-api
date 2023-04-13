using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Directive"/>.
    /// </summary>
    public sealed class DirectiveResult : SymbolResult
    {
        private List<string>? _values;

        internal DirectiveResult(CliDirective directive, CliToken token, SymbolResultTree symbolResultTree)
            : base(symbolResultTree, null) // directives don't belong to any command
        {
            Directive = directive;
            Token = token;
        }

        /// <summary>
        /// Parsed values of [name:value] directive(s).
        /// </summary>
        /// <remarks>Can be empty for [name] directives.</remarks>
        public IReadOnlyList<string> Values => _values is null ? Array.Empty<string>() : _values;

        /// <summary>
        /// The directive to which the result applies.
        /// </summary>
        public CliDirective Directive { get; }

        /// <summary>
        /// The token that was parsed to specify the directive.
        /// </summary>
        public CliToken Token { get; }

        internal void AddValue(string value) => (_values ??= new()).Add(value);
    }
}
