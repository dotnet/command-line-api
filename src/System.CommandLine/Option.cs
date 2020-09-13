﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
            : this(new[] { alias }, description)
        {
        }

        public Option(string[] aliases, string? description = null) : base(description)
        {
            if (aliases is null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            if (!aliases.Any())
            {
                throw new ArgumentException("An option must have at least one alias.");
            }

            for (var i = 0; i < aliases.Length; i++)
            {
                var alias = aliases[i];
                AddAlias(alias);
            }
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

        public void AddAlias(string alias)
        {
            // FIX: (AddAlias) 
            var unprefixedAlias = alias.RemovePrefix();

            if (string.IsNullOrWhiteSpace(unprefixedAlias))
            {
                throw new ArgumentException("An alias cannot be null, empty, or consist entirely of whitespace.");
            }

            for (var i = 0; i < alias!.Length; i++)
            {
                if (char.IsWhiteSpace(alias[i]))
                {
                    throw new ArgumentException($"{GetType().Name} alias cannot contain whitespace: \"{alias}\"");
                }
            }

            _rawAliases.Add(alias);
            _aliases.Add(unprefixedAlias!);

            OnNameOrAliasChanged?.Invoke(this);
        }

        public void AddValidator(ValidateSymbol<OptionResult> validate) => Validators.Add(validate);

        IArgument IOption.Argument => Argument;

        public bool IsRequired { get; set; }
 
        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.ValueType => Argument.ArgumentType;

        bool IValueDescriptor.HasDefaultValue => Argument.HasDefaultValue;

        object? IValueDescriptor.GetDefaultValue() => Argument.GetDefaultValue();

        private protected override string DefaultName =>
            _rawAliases
                .OrderBy(a => a.Length)
                .Last()
                .RemovePrefix();
    }
}
