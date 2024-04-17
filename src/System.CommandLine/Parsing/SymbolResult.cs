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
        // TODO: make this a property and protected if possible
        internal readonly SymbolResultTree SymbolResultTree;
        private protected List<CliToken>? _tokens;

        private protected SymbolResult(SymbolResultTree symbolResultTree, SymbolResult? parent)
        {
            SymbolResultTree = symbolResultTree;
            Parent = parent;
        }
        // TODO: this can be an extension method, do we need it?
        /*
                /// <summary>
                /// The parse errors associated with this symbol result.
                /// </summary>
                public IEnumerable<CliDiagnostic> Errors
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
        */
        /// <summary>
        /// The parent symbol result in the parse tree.
        /// </summary>
        public SymbolResult? Parent { get; }

        // TODO: make internal because exposes tokens
        /// <summary>
        /// The list of tokens associated with this symbol result during parsing.
        /// </summary>
        internal IReadOnlyList<CliToken> Tokens => _tokens is not null ? _tokens : Array.Empty<CliToken>();

        internal void AddToken(CliToken token) => (_tokens ??= new()).Add(token);

        // TODO: made nonpublic, should we make public again?
        /// <summary>
        /// Adds an error message for this symbol result to it's parse tree.
        /// </summary>
        /// <remarks>Setting an error will cause the parser to indicate an error for the user and prevent invocation of the command line.</remarks>
        internal virtual void AddError(string errorMessage) => SymbolResultTree.AddError(new CliDiagnostic(new("", "", errorMessage, severity: CliDiagnosticSeverity.Error, null), [], symbolResult: this));
        /// <summary>
        /// Finds a result for the specific argument anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="argument">The argument for which to find a result.</param>
        /// <returns>An argument result if the argument was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public ArgumentResult? GetResult(CliArgument argument) => SymbolResultTree.GetResult(argument);

        /// <summary>
        /// Finds a result for the specific command anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="command">The command for which to find a result.</param>
        /// <returns>An command result if the command was matched by the parser; otherwise, <c>null</c>.</returns>
        public CommandResult? GetResult(CliCommand command) => SymbolResultTree.GetResult(command);

        /// <summary>
        /// Finds a result for the specific option anywhere in the parse tree, including parent and child symbol results.
        /// </summary>
        /// <param name="option">The option for which to find a result.</param>
        /// <returns>An option result if the option was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public OptionResult? GetResult(CliOption option) => SymbolResultTree.GetResult(option);

        // TODO: directives
        /*
                /// <summary>
                /// Finds a result for the specific directive anywhere in the parse tree.
                /// </summary>
                /// <param name="directive">The directive for which to find a result.</param>
                /// <returns>A directive result if the directive was matched by the parser, <c>null</c> otherwise.</returns>
                public DirectiveResult? GetResult(CliDirective directive) => SymbolResultTree.GetResult(directive);
        */
        /// <summary>
        /// Finds a result for a symbol having the specified name anywhere in the parse tree.
        /// </summary>
        /// <param name="name">The name of the symbol for which to find a result.</param>
        /// <returns>An argument result if the argument was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public SymbolResult? GetResult(string name) =>
            SymbolResultTree.GetResult(name);

        /// <inheritdoc cref="ParseResult.GetValue{T}(CliArgument{T})"/>
        public T? GetValue<T>(CliArgument<T> argument)
        {
            if (GetResult(argument) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return CliArgument<T>.CreateDefaultValue();
        }

        /// <inheritdoc cref="ParseResult.GetValue{T}(CliOption{T})"/>
        public T? GetValue<T>(CliOption<T> option)
        {
            if (GetResult(option) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return CliArgument<T>.CreateDefaultValue();
        }

        /// <summary>
        /// Gets the value for a symbol having the specified name anywhere in the parse tree.
        /// </summary>
        /// <param name="name">The name of the symbol for which to find a result.</param>
        /// <returns>An argument result if the argument was matched by the parser or has a default value; otherwise, <c>null</c>.</returns>
        public T? GetValue<T>(string name)
        {
            if (GetResult(name) is { } result)
            {
                if (result is OptionResult optionResult &&
                    optionResult.GetValueOrDefault<T>() is { } optionValue)
                {
                    return optionValue;
                }

                if (result is ArgumentResult argumentResult &&
                    argumentResult.GetValueOrDefault<T>() is { } argumentValue)
                {
                    return argumentValue;
                }
            }

            return CliArgument<T>.CreateDefaultValue();
        }

        internal virtual bool UseDefaultValueFor(ArgumentResult argumentResult) => false;
    }
}
