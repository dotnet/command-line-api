// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public class Option :
        IdentifierSymbol,
        IOption
    {
        private string? _implicitName;
        private protected readonly HashSet<string> _unprefixedAliases = new HashSet<string>();

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

            if (aliases.Length == 0)
            {
                throw new ArgumentException("An option must have at least one alias.", nameof(aliases));
            }

            for (var i = 0; i < aliases.Length; i++)
            {
                var alias = aliases[i];
                AddAlias(alias);
            }
        }

        public virtual Argument Argument
        {
            get => Children.Arguments.Count > 0
                       ? Children.Arguments[0]
                       : Argument.None;
            set
            {
                for (var i = 0; i < Children.Arguments.Count; i++)
                {
                    Children.Remove(Children.Arguments[i]);
                }

                AddArgumentInner(value);
            }
        }

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

        public void AddAlias(string alias) => AddAliasInner(alias);

        private protected override void AddAliasInner(string alias)
        {
            ThrowIfAliasIsInvalid(alias);

            base.AddAliasInner(alias);

            var unprefixedAlias = alias.RemovePrefix();

            _unprefixedAliases.Add(unprefixedAlias!);
        }

        public void AddValidator(ValidateSymbol<OptionResult> validate) => Validators.Add(validate);

        public bool HasAliasIgnorePrefix(string alias) => _unprefixedAliases.Contains(alias.RemovePrefix());

        private protected override void RemoveAlias(string? alias)
        {
            _unprefixedAliases.Remove(alias!);

            base.RemoveAlias(alias);
        }

        IArgument IOption.Argument => Argument;

        public bool AllowMultipleArgumentsPerToken { get; set; } = true;

        public bool IsRequired { get; set; }

        string IValueDescriptor.ValueName => Name;

        Type IValueDescriptor.ValueType => Argument.ArgumentType;

        bool IValueDescriptor.HasDefaultValue => Argument.HasDefaultValue;

        object? IValueDescriptor.GetDefaultValue() => Argument.GetDefaultValue();

        private protected override string DefaultName =>
            _implicitName ??= Aliases
                              .OrderBy(a => a.Length)
                              .Last()
                              .RemovePrefix();
    }
}