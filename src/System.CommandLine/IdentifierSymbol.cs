// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol, such as an option or command, having one or more fixed names in a command line interface.
    /// </summary>
    public abstract class IdentifierSymbol : Symbol, IIdentifierSymbol
    {
        private readonly HashSet<string> _aliases = new();
        private string? _specifiedName;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierSymbol"/> class.
        /// </summary>
        /// <param name="description">The description of the symbol, which is displayed in command line help.</param>
        protected IdentifierSymbol(string? description = null)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierSymbol"/> class.
        /// </summary>
        /// <param name="name">The name of the symbol.</param>
        /// <param name="description">The description of the symbol, which is displayed in command line help.</param>
        protected IdentifierSymbol(string name, string? description = null) 
        {
            Name = name;
            Description = description;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Aliases => _aliases;

        /// <inheritdoc/>
        public override string Name
        {
            get => _specifiedName ?? DefaultName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
                }

                if (_specifiedName is { })
                {
                    RemoveAlias(_specifiedName);
                }

                _specifiedName = value;

                AddAliasInner(value);
            }
        }

        private protected virtual void AddAliasInner(string alias)
        {
            _aliases.Add(alias);

            OnNameOrAliasChanged?.Invoke(this);
        }

        private protected virtual void RemoveAlias(string alias)
        {
            _aliases.Remove(alias);
        }

        /// <inheritdoc />
        public virtual bool HasAlias(string alias) => _aliases.Contains(alias);
    }
}