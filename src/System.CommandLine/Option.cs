// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public class Option : 
        Symbol, 
        IOption
    {
        public Option(string alias, string? description = null)
            : base(new[]
            {
                alias
            }, description)
        {
        }

        public Option(string[] aliases, string? description = null) : base(aliases, description)
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

        internal List<ValidateSymbol<OptionResult>> Validators { get; } = new List<ValidateSymbol<OptionResult>>();

        public void AddValidator(ValidateSymbol<OptionResult> validate) => Validators.Add(validate);

        IArgument IOption.Argument => Argument;

        public bool IsRequired { get; set; }
 
        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.ValueType => Argument.ArgumentType;

        bool IValueDescriptor.HasDefaultValue => Argument.HasDefaultValue;

        object? IValueDescriptor.GetDefaultValue() => Argument.GetDefaultValue();

        private protected override void ChooseNameForUnnamedArgument(Argument argument)
        {
            argument.Name = Name.ToLower();
        }
    }
}
