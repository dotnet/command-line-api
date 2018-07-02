// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public abstract class SymbolResult
    {
        private readonly List<string> _arguments = new List<string>();
        private ArgumentParseResult _result;

        private ValidationMessages _validationMessages = ValidationMessages.Instance;

        protected internal SymbolResult(Symbol symbol, string token, CommandResult parent = null)
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

        public Symbol Symbol { get; }

        public string Token { get; }

        public IReadOnlyCollection<string> Aliases => Symbol.Aliases;

        public bool HasAlias(string alias) => Symbol.HasAlias(alias);

        public ValidationMessages ValidationMessages    
        {
            get => _validationMessages ?? (_validationMessages = ValidationMessages.Instance);
            set => _validationMessages = value;
        }

        protected internal virtual ParseError Validate()
        {
            foreach (var symbolValidator in Symbol.Argument.SymbolValidators)
            {
                var errorMessage = symbolValidator(this);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return new ParseError(errorMessage, this);
                }
            }

            _result = Symbol.Argument.Parser.Parse(this);

            switch (_result)
            {
                case FailedArgumentArityResult arityFailure:
                    return new ParseError(arityFailure.ErrorMessage,
                                          this,
                                          true);
                case FailedArgumentTypeConversionResult conversionFailure:
                    return new ParseError(conversionFailure.ErrorMessage,
                                          this,
                                          false);
                case FailedArgumentParseResult general:
                    return new ParseError(general.ErrorMessage,
                                          this,
                                          false);
            }

            return null;
        }

        internal abstract SymbolResult TryTakeToken(Token token);

        protected internal SymbolResult TryTakeArgument(Token token)
        {
            if (token.Type != TokenType.Argument)
            {
                return null;
            }

            if (!OptionWasRespecified &&
                Symbol is Option)
            {
                // Options must be respecified in order to accept additional arguments. This is
                // not the case for command.
                return null;
            }

            foreach (var child in Children)
            {
                var a = child.TryTakeToken(token);
                if (a != null)
                {
                    return a;
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
                return this;
            }

            _arguments.RemoveAt(_arguments.Count - 1);
            
            return null;
        }

        internal static SymbolResult Create(
            Symbol symbol, 
            string token, 
            CommandResult parent = null, 
            ValidationMessages validationMessages = null)
        {
            switch (symbol)
            {
                case Command command:
                    return new CommandResult(command, parent)
                    {
                        ValidationMessages = validationMessages
                    };

                case Option option:
                    return new OptionResult(option, token, parent)
                    {
                        ValidationMessages = validationMessages
                    };

                default:
                    throw new ArgumentException($"Unrecognized symbol type: {symbol.GetType()}");
            }
        }

        public ArgumentParseResult Result
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

        public override string ToString() => $"{GetType().Name}: {Token}";
    }
}
