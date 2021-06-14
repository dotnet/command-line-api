// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.Linq;

namespace System.CommandLine
{
    ///<inheritdoc/>
    public class Argument<T, TSuggestion> : Argument<T>, IArgument<TSuggestion>
        where TSuggestion : ISuggestionType<TSuggestion>, new()
    {
        private SuggestionSourceList<TSuggestion>? _genericSuggestions = null;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        public Argument(bool enforceTextMatch = true)
            : base(enforceTextMatch)
        { }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        public Argument(
            string name,
            string? description = null,
            bool enforceTextMatch = true)
            : base(name, description, enforceTextMatch)
        { }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="getDefaultValue">The delegate to invoke to return the default value.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="getDefaultValue"/> is null.</exception>
        public Argument(
            string name,
            Func<T> getDefaultValue,
            string? description = null,
            bool enforceTextMatch = true)
            : base(name, getDefaultValue, description, enforceTextMatch)
        { }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="getDefaultValue">The delegate to invoke to return the default value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="getDefaultValue"/> is null.</exception>
        public Argument(Func<T> getDefaultValue, bool enforceTextMatch = true)
            : base(getDefaultValue, enforceTextMatch)
        { }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="parse">A custom argument parser.</param>
        /// <param name="isDefault"><c>true</c> to use the <paramref name="parse"/> result as default value.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parse"/> is null.</exception>
        public Argument(
            string? name,
            ParseArgument<T> parse,
            bool isDefault = false,
            string? description = null,
            bool enforceTextMatch = true)
            : base(name, parse, isDefault, description, enforceTextMatch)
        { }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="parse">A custom argument parser.</param>
        /// <param name="isDefault"><c>true</c> to use the <paramref name="parse"/> result as default value.</param>
        public Argument(ParseArgument<T> parse, bool isDefault = false, bool enforceTextMatch = true)
            : base(parse, isDefault, enforceTextMatch)
        { }

        public SuggestionSourceList<TSuggestion> GenericSuggestions =>
            _genericSuggestions ??= new SuggestionSourceList<TSuggestion>();

        public IEnumerable<TSuggestion> GetGenericSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            var stringSuggestions = Suggestions
                .SelectMany(source => source.GetSuggestions(parseResult, textToMatch));
            var genericSuggestions = GenericSuggestions
                .SelectMany(source => source.GetGenericSuggestions(parseResult, textToMatch))
                .ToList();
            genericSuggestions.AddRange(stringSuggestions.Select(
                    suggestion => new TSuggestion().Build(parseResult, suggestion)));
            var finalSuggestions = genericSuggestions.Distinct();

            return EnforceTextMatch
                ? finalSuggestions.Containing(textToMatch ?? "")
                : finalSuggestions;
        }
    }
}
