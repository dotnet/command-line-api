// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
namespace System.CommandLine
{
    /// <inheritdoc cref="Argument" />
    public class Argument<T> : Argument
    {
        private Func<ArgumentResult, T?>? _customParser;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument. It's not used for parsing, only when displaying Help or creating parse errors.</param>>
        public Argument(string name) : base(name)
        {
        }

        /// <summary>
        /// The delegate to invoke to create the default value.
        /// </summary>
        /// <remarks>
        /// It's invoked when there was no parse input provided for given Argument.
        /// The same instance can be set as <see cref="CustomParser"/>, in such case
        /// the delegate is also invoked when an input was provided.
        /// </remarks>
        public Func<ArgumentResult, T>? DefaultValueFactory { get; set; }

        /// <summary>
        /// A custom argument parser.
        /// </summary>
        /// <remarks>
        /// It's invoked when there was parse input provided for given Argument.
        /// The same instance can be set as <see cref="DefaultValueFactory"/>, in such case
        /// the delegate is also invoked when no input was provided.
        /// </remarks>
        public Func<ArgumentResult, T?>? CustomParser
        {
            get => _customParser;
            set
            {
                _customParser = value;

                if (value is not null)
                {
                    ConvertArguments = (ArgumentResult argumentResult, out object? parsedValue) =>
                    {
                        int errorsBefore = argumentResult.SymbolResultTree.ErrorCount;
                        var result = value(argumentResult);

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
            }
        }

        /// <inheritdoc />
        public override Type ValueType => typeof(T);

        /// <inheritdoc />
        public override bool HasDefaultValue => DefaultValueFactory is not null;

        internal override object? GetDefaultValue(ArgumentResult argumentResult)
        {
            if (DefaultValueFactory is null)
            {
                throw new InvalidOperationException($"Argument \"{Name}\" does not have a default value");
            }

            return DefaultValueFactory.Invoke(argumentResult);
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
