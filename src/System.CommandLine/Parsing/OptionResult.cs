// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Option" />.
    /// </summary>
    internal sealed class OptionResult : SymbolResult
    {
        private ArgumentConversionResult? _argumentConversionResult;

        internal OptionResult(
            CliOption option,
            SymbolResultTree symbolResultTree,
            CliToken? token = null,
            CommandResult? parent = null) :
            base(symbolResultTree, parent)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            IdentifierToken = token;
        }

        private ValueResult? _valueResult;
        public ValueResult ValueResult
        {
            get
            {
                if (_valueResult is null)
                {
                    // This is not lazy on the assumption that almost everything the user enters will be used, and ArgumentResult is no longer used for defaults
                    // TODO: Make sure errors are added
                    var conversionResult = ArgumentConversionResult
                            .ConvertIfNeeded(Option.Argument.ValueType);
                    var conversionValue = conversionResult.Result switch
                    {
                        ArgumentConversionResultType.Successful => conversionResult.Value!,
                        ArgumentConversionResultType.NoArgument => default!,
                        _ => default // This is an error condition, and is handled below
                    };
                    var locations = Tokens.Select(token => token.Location).ToArray();
                    //TODO: Remove this wrapper later
                    _valueResult = new ValueResult(Option, conversionValue, locations, ArgumentResult.GetValueResultOutcome(ArgumentConversionResult?.Result), conversionResult.ErrorMessage); 
                }
                return _valueResult;
            }
        }

        /// <summary>
        /// The option to which the result applies.
        /// </summary>
        public CliOption Option { get; }

        /// <summary>
        /// Indicates whether the result was created implicitly and not due to the option being specified on the command line.
        /// </summary>
        /// <remarks>Implicit results commonly result from options having a default value.</remarks>
        public bool Implicit => IdentifierToken is null || IdentifierToken.Implicit;

        // TODO: make internal because exposes tokens
        /// <summary>
        /// The token that was parsed to specify the option.
        /// </summary>
        /// <remarks>An identifier token is a token that matches either the option's name or one of its aliases.</remarks>
        internal CliToken? IdentifierToken { get; }

        // TODO: do we even need IdentifierTokenCount
        /*
        /// <summary>
        /// The number of occurrences of an identifier token matching the option.
        /// </summary>
        public int IdentifierTokenCount { get; internal set; }
        */
        /// <inheritdoc/>
        public override string ToString() => $"{nameof(OptionResult)}: {IdentifierToken?.Value ?? Option.Name} {string.Join(" ", Tokens.Select(t => t.Value))}";

        /// <summary>
        /// Gets the parsed value or the default value for <see cref="Option"/>.
        /// </summary>
        /// <returns>The parsed value or the default value for <see cref="Option"/></returns>
        public T GetValueOrDefault<T>() =>
            ArgumentConversionResult
                .ConvertIfNeeded(typeof(T))
                .GetValueOrDefault<T>();

        internal bool IsArgumentLimitReached
            => Option.Argument.Arity.MaximumNumberOfValues == (Implicit ? Tokens.Count - 1 : Tokens.Count);

        internal ArgumentConversionResult ArgumentConversionResult
            => _argumentConversionResult ??= GetResult(Option.Argument)!.GetArgumentConversionResult();

        internal override bool UseDefaultValueFor(ArgumentResult argument) => Implicit;
    }
}
