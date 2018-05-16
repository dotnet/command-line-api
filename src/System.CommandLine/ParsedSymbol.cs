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
        private readonly List<string> arguments = new List<string>();
        private ArgumentParseResult result;

        private bool considerAcceptingAnotherArgument = true;

        protected internal ParsedSymbol(SymbolDefinition symbolDefinition, string token, ParsedCommand parent = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            SymbolDefinition = symbolDefinition ?? throw new ArgumentNullException(nameof(symbolDefinition));

            Token = token;

            Parent = parent;

            defaultValue = new Lazy<string>(SymbolDefinition.ArgumentDefinition.GetDefaultValue);
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

        public string Name => SymbolDefinition.Name;

        public ParsedCommand Parent { get; }

        public SymbolDefinition SymbolDefinition { get; }

        public string Token { get; }

        public IReadOnlyCollection<string> Aliases => SymbolDefinition.Aliases;

        public bool HasAlias(string alias) => SymbolDefinition.HasAlias(alias);

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

            result = SymbolDefinition.ArgumentDefinition.Parser.Parse(this);

            if (result is FailedArgumentParseResult failed)
            {
                return new ParseError(failed.ErrorMessage, this, false);
            }

            return null;
        }

        internal void OptionWasRespecified()
        {
            considerAcceptingAnotherArgument = true;
        }

        public abstract ParsedSymbol TryTakeToken(Token token);

        protected ParsedSymbol TryTakeArgument(Token token)
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

            arguments.Add(token.Value);

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

            arguments.RemoveAt(arguments.Count - 1);
            return null;
        }

        public override string ToString() => this.Diagram();

        internal static ParsedSymbol Create(SymbolDefinition symbolDefinition, string token, ParsedCommand parent = null)
        {
            switch (symbolDefinition)
            {
                case CommandDefinition command:
                    return new ParsedCommand(command, parent);

                case OptionDefinition option:
                    return new ParsedOption(option, token, parent);

                default:
                    throw new ArgumentException($"Unrecognized symbolDefinition type: {symbolDefinition.GetType()}");
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
