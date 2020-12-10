// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public abstract class IdentifierSymbol : Symbol, IIdentifierSymbol
    {
        private readonly HashSet<string> _aliases = new HashSet<string>();
        private string? _specifiedName;

        protected IdentifierSymbol(string? description = null)
        {
            Description = description;
        }

        protected IdentifierSymbol(string name, string? description = null) 
        {
            Name = name;
            Description = description;
        }

        public IReadOnlyCollection<string> Aliases => _aliases;

        public override string Name
        {
            get => _specifiedName ?? DefaultName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
                }

                RemoveAlias(_specifiedName);

                _specifiedName = value;

                AddAliasInner(value);
            }
        }

        private protected virtual void AddAliasInner(string alias)
        {
            _aliases.Add(alias);

            OnNameOrAliasChanged?.Invoke(this);
        }

        private protected virtual void RemoveAlias(string? alias)
        {
            if (alias != null)
            {
                _aliases.Remove(alias);
            }
        }

        public virtual bool HasAlias(string alias) => _aliases.Contains(alias);
    }
}