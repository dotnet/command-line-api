// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine
{
    public class Option : Symbol, IOption
    {
        public Option(string alias, string description = null)
            : base(new[]
            {
                alias
            }, description)
        {
        }

        public Option(string[] aliases, string description = null) : base(aliases, description)
        {
        }

        public virtual Argument Argument
        {
            get => Arguments.FirstOrDefault() ?? Argument.None;
            set
            {
                foreach (var argument in Arguments.ToArray())
                {
                    Children.Remove(argument);
                }

                AddArgumentInner(value);
            }
        }

        private IEnumerable<Argument> Arguments => Children.OfType<Argument>();

        public void AddAlias(string alias) => AddAliasInner(alias);

        internal List<ValidateSymbol<OptionResult>> Validators { get; } = new List<ValidateSymbol<OptionResult>>();

        public void AddValidator(ValidateSymbol<OptionResult> validate) => Validators.Add(validate);

        IArgument IOption.Argument => Argument;

        public bool Required { get; set; }

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.Type => Argument.ArgumentType;

        bool IValueDescriptor.HasDefaultValue => Arguments.Single().HasDefaultValue;

        object IValueDescriptor.GetDefaultValue() => Arguments.Single().GetDefaultValue();
    }
}
