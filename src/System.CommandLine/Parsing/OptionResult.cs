// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Option" />.
    /// </summary>
    public class OptionResult : SymbolResult
    {
        private ArgumentConversionResult? _argumentConversionResult;

        internal OptionResult(
            Option option,
            Token? token = null,
            CommandResult? parent = null) :
            base(option ?? throw new ArgumentNullException(nameof(option)),
                 parent)
        {
            Option = option;
            Token = token;
        }

        /// <summary>
        /// The option to which the result applies.
        /// </summary>
        public Option Option { get; }

        /// <summary>
        /// Indicates whether the result was created implicitly and not due to the option being specified on the command line.
        /// </summary>
        /// <remarks>Implicit results commonly result from options having a default value.</remarks>
        public bool IsImplicit => Token is null || Token.IsImplicit;

        /// <summary>
        /// The token that was parsed to specify the option.
        /// </summary>
        public Token? Token { get; }

        /// <inheritdoc cref="GetValueOrDefault{T}"/>
        public object? GetValueOrDefault() =>
            Option.ValueType == typeof(bool)
                ? GetValueOrDefault<bool>()
                : GetValueOrDefault<object?>();

        /// <summary>
        /// Gets the parsed value or the default value for <see cref="Option"/>.
        /// </summary>
        /// <returns>The parsed value or the default value for <see cref="Option"/></returns>
        [return: MaybeNull]
        public T GetValueOrDefault<T>() =>
            this.ConvertIfNeeded(typeof(T))
                .GetValueOrDefault<T>();

        private protected override int RemainingArgumentCapacity
        {
            get
            {
                var capacity = base.RemainingArgumentCapacity;

                if (IsImplicit && capacity < int.MaxValue)
                {
                    capacity += 1;
                }

                return capacity;
            }
        }

        internal ArgumentConversionResult ArgumentConversionResult
        {
            get
            {
                if (_argumentConversionResult is null)
                {
                    for (var i = 0; i < Children.Count; i++)
                    {
                        var child = Children[i];

                        if (child is ArgumentResult argumentResult)
                        {
                            return _argumentConversionResult = argumentResult.GetArgumentConversionResult();
                        }
                    }

                    return _argumentConversionResult = ArgumentConversionResult.None(Option.Argument);
                }

                return _argumentConversionResult;
            }
        }
        
        internal override bool UseDefaultValueFor(Argument argument) => IsImplicit;
    }
}
