// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentsRule
    {
        private readonly Func<AppliedOption, string> validate;
        private readonly Func<AppliedOption, object> materialize;
        private readonly Func<ParseResult, IEnumerable<string>> suggest;
        private readonly Lazy<string> defaultValue;

        public ArgumentsRule(Func<AppliedOption, string> validate) : this(validate, null)
        {
        }

        internal ArgumentsRule(
            Func<AppliedOption, string> validate,
            IReadOnlyCollection<string> allowedValues = null,
            Func<string> defaultValue = null,
            string description = null,
            string name = null,
            Func<ParseResult, IEnumerable<string>> suggest = null,
            Func<AppliedOption, object> materialize = null)
        {
            if (validate == null)
            {
                throw new ArgumentNullException(nameof(validate));
            }

            this.defaultValue = new Lazy<string>(defaultValue ?? (() => null));
            Description = description;
            Name = name;
            this.validate = validate;

            if (suggest == null)
            {
                this.suggest = result =>
                    AllowedValues.FindSuggestions(result);
            }
            else
            {
                this.suggest = result =>
                    suggest(result).ToArray()
                                   .FindSuggestions(
                                       result.TextToMatch());
            }

            AllowedValues = allowedValues ?? Array.Empty<string>();

            this.materialize = materialize;
        }

        public string Validate(AppliedOption option) => validate(option);

        public IReadOnlyCollection<string> AllowedValues { get; }

        public string DefaultValue => defaultValue.Value;

        internal Func<string> GetDefaultValue => () => defaultValue.Value;

        public string Description { get; }

        public string Name { get; }

        internal IEnumerable<string> Suggest(ParseResult parseResult) =>
            suggest(parseResult);

        internal object Materialize(AppliedOption appliedOption) => 
            materialize?.Invoke(appliedOption);
    }
}