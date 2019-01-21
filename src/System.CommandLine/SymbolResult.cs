// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class SymbolResult
    {
        private readonly List<string> _arguments = new List<string>();
        private ArgumentResult _result;

        private ValidationMessages _validationMessages = ValidationMessages.Instance;

        protected SymbolResult(
            ISymbol symbol, 
            string token, 
            CommandResult parent = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Token = token;

            Parent = parent;
        }

        public IReadOnlyCollection<string> Arguments => _arguments;

        public SymbolResultSet Children { get; } = new SymbolResultSet();

        public string Name => Symbol.Name;

        internal bool OptionWasRespecified { get; set; } = true;

        public CommandResult Parent { get; }

        public ISymbol Symbol { get; }

        public string Token { get; }

        public bool HasAlias(string alias) => Symbol.HasAlias(alias);

        internal bool IsArgumentLimitReached => RemainingArgumentCapacity <= 0;

        private protected virtual int RemainingArgumentCapacity =>
             Symbol.Argument.Arity.MaximumNumberOfArguments - Arguments.Count;

        public ValidationMessages ValidationMessages    
        {
            get => _validationMessages ?? (_validationMessages = ValidationMessages.Instance);
            set => _validationMessages = value;
        }

        protected internal ParseError Validate()
        {
            // TODO: (Validate) don't cast
            if (Symbol.Argument is Argument argument)
            {
                var (result, error) = argument.Validate(this);
                _result = result;
                return error;
            }

            return null;
        }

        internal abstract SymbolResult TryTakeToken(Token token);

        internal SymbolResult TryTakeArgument(Token token)
        {
            if (token.Type != TokenType.Argument)
            {
                return null;
            }

            if (!OptionWasRespecified)
            {
                if (Symbol is IOption)
                {
                    // Options must be respecified in order to accept additional arguments. This is not the case for command.
                    return null;
                }
           
                if (IsArgumentLimitReached)
                {
                    return null;
                }
            }

            _arguments.Add(token.Value);

            var parseError = Validate();

            if (parseError == null)
            {
                OptionWasRespecified = false;
                return this;
            }

            if (!parseError.CanTokenBeRetried)
            {
                OptionWasRespecified = false;
                return this;
            }

            _arguments.RemoveAt(_arguments.Count - 1);

            return null;
        }

        internal static SymbolResult Create(
            ISymbol symbol, 
            string token, 
            CommandResult parent = null, 
            ValidationMessages validationMessages = null)
        {
            switch (symbol)
            {
                case ICommand command:
                    return new CommandResult(command, parent)
                    {
                        ValidationMessages = validationMessages
                    };

                case IOption option:
                    return new OptionResult(option, token, parent)
                    {
                        ValidationMessages = validationMessages
                    };

                default:
                    throw new ArgumentException($"Unrecognized symbol type: {symbol.GetType()}");
            }
        }

        public ArgumentResult ArgumentResult
        {
            get
            {
                if (_result == null)
                {
                    Validate();
                }

                return _result;
            }
            protected set => _result = value;
        }

        internal bool UseDefaultValue { get; set; }

        public override string ToString() => $"{GetType().Name}: {Token}";
    }
}
