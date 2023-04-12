﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced during parsing for a specific symbol.
    /// </summary>
    public abstract class SymbolResult
    {
        internal readonly SymbolResultTree SymbolResultTree;
        private protected List<Token>? _tokens;

        private protected SymbolResult(SymbolResultTree symbolResultTree, SymbolResult? parent)
        {
            SymbolResultTree = symbolResultTree;
            Parent = parent;
        }

        /// <summary>
        /// The parse errors associated with this symbol result.
        /// </summary>
        public IEnumerable<ParseError> Errors
        {
            get
            {
                var parseErrors = SymbolResultTree.Errors;

                if (parseErrors is null)
                {
                    yield break;
                }

                for (var i = 0; i < parseErrors.Count; i++)
                {
                    var parseError = parseErrors[i];
                    if (parseError.SymbolResult == this)
                    {
                        yield return parseError;
                    }
                }
            }
        }

        /// <summary>
        /// The parent symbol result in the parse tree.
        /// </summary>
        public SymbolResult? Parent { get; }

        /// <summary>
        /// The list of tokens associated with this symbol result during parsing.
        /// </summary>
        public IReadOnlyList<Token> Tokens => _tokens is not null ? _tokens : Array.Empty<Token>();

        internal void AddToken(Token token) => (_tokens ??= new()).Add(token);

        /// <summary>
        /// Adds an error message for this symbol result to it's parse tree.
        /// </summary>
        /// <remarks>Setting an error will cause the parser to indicate an error for the user and prevent invocation of the command line.</remarks>
        public virtual void AddError(string errorMessage) => SymbolResultTree.AddError(new ParseError(errorMessage, this));

        /// <summary>
        /// Finds a result for the specific argument anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="argument">The argument for which to find a result.</param>
        /// <returns>An argument result if the argument was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public ArgumentResult? FindResultFor(CliArgument argument) => SymbolResultTree.FindResultFor(argument);

        /// <summary>
        /// Finds a result for the specific command anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="command">The command for which to find a result.</param>
        /// <returns>An command result if the command was matched by the parser; otherwise, <c>null</c>.</returns>
        public CommandResult? FindResultFor(CliCommand command) => SymbolResultTree.FindResultFor(command);

        /// <summary>
        /// Finds a result for the specific option anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="option">The option for which to find a result.</param>
        /// <returns>An option result if the option was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public OptionResult? FindResultFor(CliOption option) => SymbolResultTree.FindResultFor(option);

        /// <summary>
        /// Finds a result for the specific directive anywhere in the parse tree.
        /// </summary>
        /// <param name="directive">The directive for which to find a result.</param>
        /// <returns>A directive result if the directive was matched by the parser, <c>null</c> otherwise.</returns>
        public DirectiveResult? FindResultFor(CliDirective directive) => SymbolResultTree.FindResultFor(directive);

        /// <inheritdoc cref="ParseResult.GetValue{T}(CliArgument{T})"/>
        public T? GetValue<T>(CliArgument<T> argument)
        {
            if (FindResultFor(argument) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return CliArgument<T>.CreateDefaultValue();
        }

        /// <inheritdoc cref="ParseResult.GetValue{T}(CliOption{T})"/>
        public T? GetValue<T>(CliOption<T> option)
        {
            if (FindResultFor(option) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return CliArgument<T>.CreateDefaultValue();
        }

        internal virtual bool UseDefaultValueFor(ArgumentResult argumentResult) => false;
    }
}
