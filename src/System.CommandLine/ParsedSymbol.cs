// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public abstract class ParsedSymbol
    {
        private readonly Lazy<string> defaultValue;
        protected internal readonly List<string> arguments = new List<string>();
        private ArgumentParseResult result;

        private bool considerAcceptingAnotherArgument = true;

        protected internal ParsedSymbol(Symbol symbol, string token, ParsedCommand parent = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Token = token;

            Parent = parent;

            defaultValue = new Lazy<string>(Symbol.ArgumentsRule.GetDefaultValue);
        }
        
        public IReadOnlyCollection<string> Arguments
        {
            get
            {
                if (!arguments.Any() &&
                    defaultValue.Value != null)
                {
                    return new[] { defaultValue.Value };
                }

                return arguments;
            }
        }

        public ParsedSymbolSet Children { get; } = new ParsedSymbolSet();

        public string Name => Symbol.Name;

        public ParsedCommand Parent { get; }

        public Symbol Symbol { get; }

        public string Token { get; }

        public IReadOnlyCollection<string> Aliases => Symbol.Aliases;

        public bool HasAlias(string alias) => Symbol.HasAlias(alias);

        internal ParseError Validate()
        {
            foreach (var symbolValidator in Symbol.ArgumentsRule.SymbolValidators)
            {
                var error = symbolValidator(this);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    return new ParseError(error, Token, this);
                }
            }

            var argumentParser = Symbol.ArgumentsRule.Parser;

            if (argumentParser == null)
            {
                return null;
            }

            result = argumentParser.Parse(this);

            if (result is FailedArgumentParseResult failed)
            {
                return new ParseError(failed.ErrorMessage, Token, this);
            }

            return null;
        }

        internal void OptionWasRespecified() => considerAcceptingAnotherArgument = true;

        public abstract ParsedSymbol TryTakeToken(Token token);

        protected ParsedSymbol TryTakeArgument(Token token)
        {
            if (token.Type != TokenType.Argument)
            {
                return null;
            }
            
            if (!considerAcceptingAnotherArgument &&
                !(Symbol is Command))
            {
                // Options must be respecified in order to accept additional arguments. This is 
                // not the case for commands.
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

            arguments.Add(token.Value);

            if (Validate() == null)
            {
                considerAcceptingAnotherArgument = false;
                return this;
            }

            arguments.RemoveAt(arguments.Count - 1);
            return null;
        }

        public override string ToString() => this.Diagram();

        internal static ParsedSymbol Create(Symbol symbol, string token, ParsedCommand parent = null)
        {
            switch (symbol)
            {
                case Command command:
                    return new ParsedCommand(command, parent);

                case Option option:
                    return new ParsedOption(option, token, parent);

                default:
                    throw new ArgumentException($"Unrecognized symbol type: {symbol.GetType()}");
            }
        }

        public ArgumentParseResult Result
        {
            get
            {
                if (result == null)
                {
                    Validate();
                }
                return result;
            }
        }
    }
}
