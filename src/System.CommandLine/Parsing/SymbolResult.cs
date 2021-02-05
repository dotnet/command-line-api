﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    public abstract class SymbolResult
    {
        private protected readonly List<Token> _tokens = new List<Token>();
        private ValidationMessages? _validationMessages;
        private readonly Dictionary<IArgument, ArgumentResult> _defaultArgumentValues = new Dictionary<IArgument, ArgumentResult>();

        private protected SymbolResult(
            ISymbol symbol, 
            SymbolResult? parent)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Parent = parent;

            Root = parent?.Root;
        }

        public string? ErrorMessage { get; set; }

        public SymbolResultSet Children { get; } = new SymbolResultSet();

        public SymbolResult? Parent { get; }

        internal virtual RootCommandResult? Root { get; }

        public ISymbol Symbol { get; }

        public IReadOnlyList<Token> Tokens => _tokens;

        internal bool IsArgumentLimitReached => RemainingArgumentCapacity == 0;

        private protected virtual int RemainingArgumentCapacity =>
            MaximumArgumentCapacity() - Tokens.Count;

        internal int MaximumArgumentCapacity()
        {
            var value = 0;

            var arguments = Symbol.Arguments();
            
            for (var i = 0; i < arguments.Count; i++)
            {
                value += arguments[i].Arity.MaximumNumberOfValues;
            }

            return value;
        }

        protected internal ValidationMessages ValidationMessages
        {
            get => _validationMessages ??= Parent?.ValidationMessages ?? ValidationMessages.Instance;
            set => _validationMessages = value;
        }

        internal void AddToken(Token token) => _tokens.Add(token);

        public virtual ArgumentResult? FindResultFor(IArgument argument) =>
            Root?.FindResultFor(argument);

        public virtual CommandResult? FindResultFor(ICommand command) =>
            Root?.FindResultFor(command);

        public virtual OptionResult? FindResultFor(IOption option) =>
            Root?.FindResultFor(option);

        internal ArgumentResult GetOrCreateDefaultArgumentResult(Argument argument) =>
            _defaultArgumentValues.GetOrAdd(
                argument,
                arg => new ArgumentResult(
                    argument,
                    this));

        internal virtual bool UseDefaultValueFor(IArgument argument) => false;

        public override string ToString() => $"{GetType().Name}: {this.Token()}";

        internal ParseError? UnrecognizedArgumentError(Argument argument)
        {
            if (argument.AllowedValues?.Count > 0 &&
                Tokens.Count > 0)
            {
                for (var i = 0; i < Tokens.Count; i++)
                {
                    var token = Tokens[i];
                    if (!argument.AllowedValues.Contains(token.Value))
                    {
                        return new ParseError(
                            ValidationMessages
                                .UnrecognizedArgument(token.Value, argument.AllowedValues),
                            this);
                    }
                }
            }

            return null;
        }
    }
}
