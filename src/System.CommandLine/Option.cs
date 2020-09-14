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
        private string? _implicitName;

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

        public override string Name
        {
            get => base.Name;
            set
            {
                if (!HasAlias(value))
                {
                    _implicitName = null;
                    RemoveAlias(DefaultName);
                }

                base.Name = value;
            }
        }

        internal List<ValidateSymbol<OptionResult>> Validators { get; } = new List<ValidateSymbol<OptionResult>>();

        public void AddAlias(string alias)
        {
            AddAliasInner(alias);
        }

        private protected override void AddAliasInner(string alias)
        {
            ThrowIfAliasIsInvalid(alias);

            var unprefixedAlias = alias.RemovePrefix();

            _rawAliases.Add(alias);
            _aliases.Add(unprefixedAlias!);

            base.AddAliasInner(alias);
        }

        public void AddValidator(ValidateSymbol<OptionResult> validate) => Validators.Add(validate);

        public override bool HasAlias(string alias) => base.HasAlias(alias.RemovePrefix());

        IArgument IOption.Argument => Argument;

        public bool IsRequired { get; set; }

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.ValueType => Argument.ArgumentType;

        bool IValueDescriptor.HasDefaultValue => Argument.HasDefaultValue;

        object? IValueDescriptor.GetDefaultValue() => Argument.GetDefaultValue();

        private protected override string DefaultName =>
            _implicitName ??= _rawAliases
                              .OrderBy(a => a.Length)
                              .Last()
                              .RemovePrefix();
    }
}