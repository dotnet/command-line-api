// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class SymbolResult
    {
        private readonly List<Token> _tokens = new List<Token>();
        private ArgumentResult _result;

        private ValidationMessages _validationMessages = ValidationMessages.Instance;

        protected SymbolResult(
            ISymbol symbol, 
            Token token, 
            CommandResult parent = null)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Token = token ?? throw new ArgumentNullException(nameof(token));

            Parent = parent;
        }

        [Obsolete("Use the Tokens property instead. The Arguments property will be removed in a later version.")]
        public IReadOnlyCollection<string> Arguments => _tokens.Select(t => t.Value).ToArray();

        public IReadOnlyCollection<Token> Tokens => _tokens;

        public SymbolResultSet Children { get; } = new SymbolResultSet();

        public string Name => Symbol.Name;

        internal bool OptionWasRespecified { get; set; } = true;

        public CommandResult Parent { get; }

        public ISymbol Symbol { get; }

        public Token Token { get; }

        public bool HasAlias(string alias) => Symbol.HasAlias(alias);

        internal bool IsArgumentLimitReached => RemainingArgumentCapacity <= 0;

        private protected virtual int RemainingArgumentCapacity =>
            Symbol.Arguments().Sum(a => a.Arity.MaximumNumberOfValues) - Arguments.Count;

        public ValidationMessages ValidationMessages    
        {
            get => _validationMessages ?? (_validationMessages = ValidationMessages.Instance);
            set => _validationMessages = value;
        }

        protected internal IReadOnlyCollection<ParseError> Validate()
        {
            var errors = new List<ParseError>();

            var arguments = Symbol.Arguments();

            if (arguments.Count == 0)
            {
                arguments = new IArgument[]
                {
                    Argument.None
                };
            }

            foreach (var argument in arguments)
            {
                if (argument is Argument arg)
                {
                    var (result, error) = arg.Validate(this);

                    // FIX: (Validate) cardinality / side effect
                    _result = result;

                    if (error != null)
                    {
                        errors.Add(error);
                    }
                }
            }

            return errors;
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
                if (IsArgumentLimitReached)
                {
                    return null;
                }
            }

            _tokens.Add(token);

            var parseError = Validate().SingleOrDefault();

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

            if (_result is MissingArgumentResult)
            {
                OptionWasRespecified = false;
                return this;
            }

            _tokens.RemoveAt(_tokens.Count - 1);

            return null;
        }

        internal static SymbolResult Create(
            ISymbol symbol,
            Token token,
            CommandResult parent = null, 
            ValidationMessages validationMessages = null)
        {
            switch (symbol)
            {
                case ICommand command:
                    return new CommandResult(command, token, parent)
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

        private readonly HashSet<IArgument> _argumentsUsingDefaultValue = new HashSet<IArgument>();

        internal bool UseDefaultValueFor(IArgument argument)
        {
            return _argumentsUsingDefaultValue.Contains(argument);
        }

        internal void UseDefaultValueFor(IArgument argument, bool value)
        {
            if (value)
            {
                _argumentsUsingDefaultValue.Add(argument);
            }
            else
            {
                _argumentsUsingDefaultValue.Remove(argument);
            }
        }

        public override string ToString() => $"{GetType().Name}: {Token}";
    }
}
