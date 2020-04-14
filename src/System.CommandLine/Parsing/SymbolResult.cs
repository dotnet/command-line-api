// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

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
        }

        public string? ErrorMessage { get; set; }

        public SymbolResultSet Children { get; } = new SymbolResultSet();

        public SymbolResult? Parent { get; }

        public ISymbol Symbol { get; }

        public IReadOnlyList<Token> Tokens => _tokens;

        internal bool IsArgumentLimitReached => RemainingArgumentCapacity <= 0;

        private protected virtual int RemainingArgumentCapacity =>
            MaximumArgumentCapacity() - Tokens.Count;

        internal int MaximumArgumentCapacity() =>
            Symbol.Arguments()
                  .Sum(a => a.Arity.MaximumNumberOfValues);

        protected internal ValidationMessages ValidationMessages
        {
            get
            {
                if (_validationMessages == null)
                {
                    if (Parent == null)
                    {
                        _validationMessages = ValidationMessages.Instance;
                    }
                    else
                    {
                        _validationMessages = Parent.ValidationMessages;
                    }
                }

                return _validationMessages;
            }
            set => _validationMessages = value;
        }

        internal void AddToken(Token token) => _tokens.Add(token);

        internal ArgumentResult GetOrCreateDefaultArgumentResult(Argument argument) =>
            _defaultArgumentValues.GetOrAdd(
                argument,
                arg => new ArgumentResult(
                    argument,
                    this));

        internal bool UseDefaultValueFor(IArgument argument)
        {
            switch (this)
            {
                case OptionResult optionResult
                    when optionResult.IsImplicit:
                    return true;

                case CommandResult _
                    when Children.ResultFor(argument)?.Tokens.Count == 0:
                    return true;

                default:
                    return false;
            }
        }

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
