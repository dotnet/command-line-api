// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced during parsing for a specific symbol.
    /// </summary>
    public abstract class SymbolResult
    {
        private protected readonly List<Token> _tokens = new();
        private Resources? _resources;
        private readonly Dictionary<IArgument, ArgumentResult> _defaultArgumentValues = new();

        private protected SymbolResult(
            ISymbol symbol, 
            SymbolResult? parent)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Parent = parent;

            Root = parent?.Root;
        }

        /// <summary>
        /// An error message for this symbol result.
        /// </summary>
        /// <remarks>Setting this value to a non-<c>null</c> during parsing will cause the parser to indicate an error for the user and prevent invocation of the command line.</remarks>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Child symbol results in the parse tree.
        /// </summary>
        public SymbolResultSet Children { get; } = new();

        /// <summary>
        /// The parent symbol result in the parse tree.
        /// </summary>
        public SymbolResult? Parent { get; }

        internal virtual RootCommandResult? Root { get; }

        /// <summary>
        /// The symbol to which the result applies.
        /// </summary>
        public ISymbol Symbol { get; }

        /// <summary>
        /// The list of tokens associated with this symbol result during parsing.
        /// </summary>
        public IReadOnlyList<Token> Tokens => _tokens;

        internal bool IsArgumentLimitReached => RemainingArgumentCapacity == 0;

        private protected virtual int RemainingArgumentCapacity =>
            MaximumArgumentCapacity() - Tokens.Count;

        internal int MaximumArgumentCapacity()
        {
            var value = 0;

            var arguments = Symbol.Arguments();

            for (var i = 0; i < arguments.Count; i++)
            {
                value += arguments[i].Arity.MaximumNumberOfValues;
            }

            return value;
        }

        /// <summary>
        /// Localization resources used to produce messages for this symbol result.
        /// </summary>
        protected internal Resources Resources
        {
            get => _resources ??= Parent?.Resources ?? Resources.Instance;
            set => _resources = value;
        }

        internal void AddToken(Token token) => _tokens.Add(token);

        /// <summary>
        /// Finds a result for the specific argument anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="argument">The argument for which to find a result.</param>
        /// <returns>An argument result if the argument was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public virtual ArgumentResult? FindResultFor(IArgument argument) =>
            Root?.FindResultFor(argument);

        /// <summary>
        /// Finds a result for the specific command anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="command">The command for which to find a result.</param>
        /// <returns>An command result if the command was matched by the parser; otherwise, <c>null</c>.</returns>
        public virtual CommandResult? FindResultFor(ICommand command) =>
            Root?.FindResultFor(command);

        /// <summary>
        /// Finds a result for the specific option anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="option">The option for which to find a result.</param>
        /// <returns>An option result if the option was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public virtual OptionResult? FindResultFor(IOption option) =>
            Root?.FindResultFor(option);

        internal ArgumentResult GetOrCreateDefaultArgumentResult(Argument argument) =>
            _defaultArgumentValues.GetOrAdd(
                argument,
                arg => new ArgumentResult(
                    arg,
                    this));

        internal virtual bool UseDefaultValueFor(IArgument argument) => false;

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name}: {this.Token()}";

        internal ParseError? UnrecognizedArgumentError(Argument argument)
        {
            if (argument.AllowedValues?.Count > 0 &&
                Tokens.Count > 0)
            {
                for (var i = 0; i < Tokens.Count; i++)
                {
                    var token = Tokens[i];
                    if (!argument.AllowedValues.Contains(token.Value))
                    {
                        return new ParseError(
                            Resources
                                .UnrecognizedArgument(token.Value, argument.AllowedValues),
                            this);
                    }
                }
            }

            return null;
        }
    }
}
