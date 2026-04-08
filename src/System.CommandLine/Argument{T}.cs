// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
namespace System.CommandLine
{
    /// <inheritdoc cref="Argument" />
    public class Argument<T> : Argument
    {
        private Func<ArgumentResult, T?>? _customParser;
        private Func<ArgumentResult, T>? _defaultValueFactory;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument. This name can be used to look up the parsed value and is displayed in help.</param>
        public Argument(string name) : base(name)
        {
        }

        private static TryConvertArgument CreateConverter(Func<ArgumentResult, T?> valueFactory)
        {
            return (ArgumentResult argumentResult, out object? parsedValue) =>
            {
                int errorsBefore = argumentResult.SymbolResultTree.ErrorCount;
                var result = valueFactory(argumentResult);

                if (errorsBefore == argumentResult.SymbolResultTree.ErrorCount)
                {
                    parsedValue = result;
                    return true;
                }
                else
                {
                    parsedValue = default(T)!;
                    return false;
                }
            };
        }

        private void SetCustomParser(Func<ArgumentResult, T?>? value)
        {
            _customParser = value;
            ConvertArguments = value is null ? null : CreateConverter(value);
        }

        /// <summary>
        /// Sets a delegate that will be invoked to produce the argument's value.
        /// </summary>
        /// <param name="valueFactory">The delegate to invoke to produce the value.</param>
        /// <param name="invocation">
        /// Specifies when the delegate should be invoked. Use <see cref="ValueFactoryInvocation.Always"/>
        /// to handle both explicit values and missing-value defaults with the same delegate.
        /// </param>
        public void SetValueFactory(
            Func<ArgumentResult, T> valueFactory,
            ValueFactoryInvocation invocation = ValueFactoryInvocation.WhenTokensMatched)
        {
            if (valueFactory is null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            if (invocation == 0 ||
                (invocation & ~ValueFactoryInvocation.Always) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(invocation));
            }

            _defaultValueFactory = (invocation & ValueFactoryInvocation.WhenTokensNotMatched) != 0
                ? valueFactory
                : null;

            SetCustomParser(
                (invocation & ValueFactoryInvocation.WhenTokensMatched) != 0
                    ? result => valueFactory(result)
                    : null);
        }

        /// <summary>
        /// Gets or sets the delegate to invoke to create the default value.
        /// </summary>
        /// <remarks>
        /// This delegate is invoked when there was no parse input provided for given Argument.
        /// The same instance can be set as <see cref="CustomParser"/>. In that case,
        /// the delegate is also invoked when an input was provided.
        /// </remarks>
        [Obsolete($"Use SetValueFactory(..., {nameof(ValueFactoryInvocation)}.{nameof(ValueFactoryInvocation.WhenTokensNotMatched)}) instead.")]
        public Func<ArgumentResult, T>? DefaultValueFactory
        {
            get
            {
                if (_defaultValueFactory is null && this is Argument<bool>)
                {
                    _defaultValueFactory = _ => (T)(object)false;
                }

                return _defaultValueFactory;
            }
            set => _defaultValueFactory = value;
        }

        /// <summary>
        /// Gets or sets a custom argument parser.
        /// </summary>
        /// <remarks>
        /// The custom parser is invoked when there was parse input provided for a given Argument.
        /// The same instance can be set as <see cref="DefaultValueFactory"/>; in that case,
        /// the delegate is also invoked when no input was provided.
        /// </remarks>
        [Obsolete($"Use SetValueFactory(..., {nameof(ValueFactoryInvocation)}.{nameof(ValueFactoryInvocation.WhenTokensMatched)}) instead.")]
        public Func<ArgumentResult, T?>? CustomParser
        {
            get => _customParser;
            set => SetCustomParser(value);
        }

        /// <inheritdoc />
        public override Type ValueType => typeof(T);

        /// <inheritdoc />
        public override bool HasDefaultValue => _defaultValueFactory is not null || this is Argument<bool>;

        internal override object? GetDefaultValue(ArgumentResult argumentResult)
        {
            if (_defaultValueFactory is null)
            {
                if (this is Argument<bool>)
                {
                    return false;
                }

                throw new InvalidOperationException($"Argument \"{Name}\" does not have a default value");
            }

            return _defaultValueFactory.Invoke(argumentResult);
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "https://github.com/dotnet/command-line-api/issues/1638")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2091", Justification = "https://github.com/dotnet/command-line-api/issues/1638")]
        internal static T? CreateDefaultValue()
        {
            if (default(T) is null && typeof(T) != typeof(string))
            {
#if NET7_0_OR_GREATER
                if (typeof(T).IsSZArray)
#else
                if (typeof(T).IsArray && typeof(T).GetArrayRank() == 1)
#endif
                {
                    return (T?)(object)Array.CreateInstance(typeof(T).GetElementType()!, 0);
                }
                else if (typeof(T).IsConstructedGenericType)
                {
                    var genericTypeDefinition = typeof(T).GetGenericTypeDefinition();

                    if (genericTypeDefinition == typeof(IEnumerable<>) ||
                        genericTypeDefinition == typeof(IList<>) ||
                        genericTypeDefinition == typeof(ICollection<>))
                    {
                        return (T?)(object)Array.CreateInstance(typeof(T).GenericTypeArguments[0], 0);
                    }

                    if (genericTypeDefinition == typeof(List<>))
                    {
                        return Activator.CreateInstance<T>();
                    }
                }
            }

            return default;
        }
    }
}
