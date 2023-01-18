// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing an <see cref="Option" />.
    /// </summary>
    public sealed class OptionResult : SymbolResult
    {
        private List<ArgumentResult>? _children;
        private ArgumentConversionResult? _argumentConversionResult;
        private Dictionary<Argument, ArgumentResult>? _defaultArgumentValues;

        internal OptionResult(
            Option option,
            Token? token = null,
            CommandResult? parent = null) :
            base(parent)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
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

        internal override int MaximumArgumentCapacity => Option.Argument.Arity.MaximumNumberOfValues;

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
                    if (_children is not null)
                    {
                        return _argumentConversionResult = _children[0].GetArgumentConversionResult();
                    }

                    return _argumentConversionResult = ArgumentConversionResult.None(Option.Argument);
                }

                return _argumentConversionResult;
            }
        }

        internal void AddChild(ArgumentResult argumentResult) => (_children ??= new()).Add(argumentResult);

        internal override bool UseDefaultValueFor(Argument argument) => IsImplicit;

        internal ArgumentResult GetOrCreateDefaultArgumentResult(Argument argument) =>
            (_defaultArgumentValues ??= new()).GetOrAdd(
                argument,
                arg => new ArgumentResult(arg, this));
    }
}
