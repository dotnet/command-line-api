// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class Symbol
    {
        private readonly Lazy<string> _defaultValue;
        private readonly List<string> _arguments = new List<string>();
        private ArgumentParseResult _result;

        private bool considerAcceptingAnotherArgument = true;

        protected internal Symbol(SymbolDefinition symbolDefinition, string token, Command parent = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            SymbolDefinition = symbolDefinition ?? throw new ArgumentNullException(nameof(symbolDefinition));

            Token = token;

            Parent = parent;

            _defaultValue = new Lazy<string>(SymbolDefinition.ArgumentDefinition.GetDefaultValue);
        }

        public IReadOnlyCollection<string> Arguments
        {
            get
            {
                if (!_arguments.Any() &&
                    _defaultValue.Value != null)
                {
                    return new[] { _defaultValue.Value };
                }

                return _arguments;
            }
        }

        public SymbolSet Children { get; } = new SymbolSet();

        public string Name => SymbolDefinition.Name;

        public Command Parent { get; }

        public SymbolDefinition SymbolDefinition { get; }

        public string Token { get; }

        public IReadOnlyCollection<string> Aliases => SymbolDefinition.Aliases;

        public bool HasAlias(string alias) => SymbolDefinition.HasAlias(alias);

        public ValidationMessages ValidationMessages { get; private set; } = ValidationMessages.Instance;

        protected internal virtual ParseError Validate()
        {
            foreach (var symbolValidator in SymbolDefinition.ArgumentDefinition.SymbolValidators)
            {
                var errorMessage = symbolValidator(this);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return new ParseError(errorMessage, this);
                }
            }

            _result = SymbolDefinition.ArgumentDefinition.Parser.Parse(this);

            if (_result is FailedArgumentParseResult failed)
            {
                return new ParseError(failed.ErrorMessage, this, false);
            }

            return null;
        }

        internal void OptionWasRespecified()
        {
            considerAcceptingAnotherArgument = true;
        }

        public abstract Symbol TryTakeToken(Token token);

        protected Symbol TryTakeArgument(Token token)
        {
            if (token.Type != TokenType.Argument)
            {
                return null;
            }

            if (!considerAcceptingAnotherArgument &&
                !(SymbolDefinition is CommandDefinition))
            {
                // Options must be respecified in order to accept additional arguments. This is
                // not the case for commandDefinitions.
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
                considerAcceptingAnotherArgument = false;
                return this;
            }

            if (!parseError.CanTokenBeRetried)
            {
                return this;
            }

            _arguments.RemoveAt(_arguments.Count - 1);
            return null;
        }

        internal static Symbol Create(SymbolDefinition symbolDefinition, string token, Command parent = null, ValidationMessages validationMessages = null)
        {
            switch (symbolDefinition)
            {
                case CommandDefinition command:
                    return new Command(command, parent)
                    {
                        ValidationMessages = validationMessages
                    };

                case OptionDefinition option:
                    return new Option(option, token, parent)
                    {
                        ValidationMessages = validationMessages
                    };

                default:
                    throw new ArgumentException($"Unrecognized symbolDefinition type: {symbolDefinition.GetType()}");
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
        }
    }
}
