// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentsRule
    {
        private readonly Func<Parsed, string> validate;
        private readonly Func<Parsed, object> materialize;
        private readonly Suggest suggest;
        private readonly Func<string> defaultValue;

        public ArgumentsRule(Func<Parsed, string> validate) : this(validate, null)
        {
        }

        internal ArgumentsRule(
            Func<Parsed, string> validate,
            IReadOnlyCollection<string> allowedValues = null,
            Func<string> defaultValue = null,
            string description = null,
            string name = null,
            Suggest suggest = null,
            Func<Parsed, object> materialize = null)
        {
            this.validate = validate ?? throw new ArgumentNullException(nameof(validate));

            this.defaultValue = defaultValue;
            Description = description;
            Name = name;

            if (suggest == null)
            {
                this.suggest = (result, position) =>
                    AllowedValues.FindSuggestions(
                        result,
                        position ?? result.ImplicitCursorPosition());
            }
            else
            {
                this.suggest = (result, position) =>
                    suggest(result).ToArray()
                                   .FindSuggestions(
                                       result.TextToMatch(position ?? result.ImplicitCursorPosition()));
            }

            AllowedValues = allowedValues ?? Array.Empty<string>();

            this.materialize = materialize;
        }

        public string Validate(Parsed option) => validate(option);

        public IReadOnlyCollection<string> AllowedValues { get; }

        internal Func<string> GetDefaultValue => () => defaultValue?.Invoke();

        public string Description { get; }

        public string Name { get; }

        internal Func<Parsed, object> Materializer => materialize;

        internal bool HasDefaultValue => defaultValue != null;

        internal IEnumerable<string> Suggest(ParseResult parseResult, int? position = null) =>
            suggest(parseResult, position);

        internal object Materialize(Parsed parsedOption) =>
            materialize?.Invoke(parsedOption);
    }
}
