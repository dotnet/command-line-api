﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Option" />.
    /// </summary>
    public sealed class OptionResult : SymbolResult
    {
        private ArgumentConversionResult? _argumentConversionResult;

        internal OptionResult(
            Option option,
            SymbolResultTree symbolResultTree,
            Token? token = null,
            CommandResult? parent = null) :
            base(symbolResultTree, parent)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            IdentifierToken = token;
        }

        /// <summary>
        /// The option to which the result applies.
        /// </summary>
        public Option Option { get; }

        /// <summary>
        /// Indicates whether the result was created implicitly and not due to the option being specified on the command line.
        /// </summary>
        /// <remarks>Implicit results commonly result from options having a default value.</remarks>
        public bool IsImplicit => IdentifierToken is null || IdentifierToken.IsImplicit;

        /// <summary>
        /// The token that was parsed to specify the option.
        /// </summary>
        public Token? IdentifierToken { get; }

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(OptionResult)}: {IdentifierToken?.Value ?? Option.Name} {string.Join(" ", Tokens.Select(t => t.Value))}";

        /// <summary>
        /// Gets the parsed value or the default value for <see cref="Option"/>.
        /// </summary>
        /// <returns>The parsed value or the default value for <see cref="Option"/></returns>
        [return: MaybeNull]
        public T GetValueOrDefault<T>() =>
            ArgumentConversionResult.ConvertIfNeeded(typeof(T))
                .GetValueOrDefault<T>();

        internal bool IsArgumentLimitReached
            => Option.Argument.Arity.MaximumNumberOfValues == (IsImplicit ? Tokens.Count - 1 : Tokens.Count);

        internal ArgumentConversionResult ArgumentConversionResult
            => _argumentConversionResult ??= FindResultFor(Option.Argument)!.GetArgumentConversionResult();

        internal override bool UseDefaultValueFor(ArgumentResult argument) => IsImplicit;
    }
}
