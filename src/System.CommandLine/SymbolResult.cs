// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public abstract class SymbolResult
    {
        private protected readonly List<Token> _tokens = new List<Token>();

        private ValidationMessages _validationMessages;

        private readonly Dictionary<IArgument, object> _defaultArgumentValues = new Dictionary<IArgument, object>();

        private ArgumentResultSet _argumentResults = new ArgumentResultSet();

        private protected SymbolResult(
            ISymbol symbol, 
            Token token, 
            SymbolResult parent = null)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Token = token ?? throw new ArgumentNullException(nameof(token));

            Parent = parent;
        }

        [Obsolete("Use the ArgumentResults property instead")]
        internal ArgumentResult ArgumentResult
        {
            get
            {
                var argument =  Symbol switch {
                    IOption o => o.Argument,
                    ICommand c => c.Argument,
                    IArgument a => a
                };


                return ArgumentResults.SingleOrDefault() ??
                       ArgumentResult.None(argument);
            }
        }

        internal ArgumentResultSet ArgumentResults
        {
            get
            {
                // FIX: (ArgumentResults) 
                if (CommandLineConfiguration.UseNewParser)
                {
                    var results = Children
                                  .OfType<ArgumentResult2>()
                                  .Select(r => Parse(r, r.Argument));

                    var resultSet = new ArgumentResultSet();

                    foreach (var result in results)
                    {
                        resultSet.Add(result);
                    }

                    return resultSet;
                }
                else
                {
                    return _argumentResults;
                }
            }
            private set => _argumentResults = value;
        }

        [Obsolete("Use the Tokens property instead. The Arguments property will be removed in a later version.")]
        public IReadOnlyCollection<string> Arguments => 
            Tokens.Select(t => t.Value).ToArray();

        public string ErrorMessage { get; set; }

        public SymbolResultSet Children { get; } = new SymbolResultSet();

        public string Name => Symbol.Name;

        internal bool OptionWasRespecified { get; set; } = true;

        public SymbolResult Parent { get; }

        public CommandResult ParentCommandResult => Parent as CommandResult;

        public ISymbol Symbol { get; }

        public Token Token { get; }

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

        [Obsolete]
        internal abstract SymbolResult TryTakeToken(Token token);

        [Obsolete]
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

            if (ArgumentResults.Any(r => r is MissingArgumentResult))
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

        internal object GetDefaultValueFor(IArgument argument)
        {
            return _defaultArgumentValues.GetOrAdd(
                argument,
                a => a.GetDefaultValue());
        }

        internal bool UseDefaultValueFor(IArgument argument)
        {
            if (CommandLineConfiguration.UseNewParser)
            {
                if (this is OptionResult optionResult && 
                    optionResult.IsImplicit)
                {
                    return true;
                }

                if (this is CommandResult &&
                    Children.ResultFor(argument)?.Token is ImplicitToken)
                {
                    return true;
                }
            }

            return _defaultArgumentValues.ContainsKey(argument);
        }

        internal void UseDefaultValueFor(IArgument argument, bool value)
        {
            if (value)
            {
                _defaultArgumentValues.Add(argument, argument.GetDefaultValue());
            }
            else
            {
                _defaultArgumentValues.Remove(argument);
            }
        }

        public override string ToString() => $"{GetType().Name}: {Token}";

        [Obsolete]
        protected internal IReadOnlyCollection<ParseError> Validate()
        {
            var errors = new List<ParseError>();

            var arguments = Symbol.Arguments();
            ArgumentResults = new ArgumentResultSet();

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
                    var (result, error) = Validate(arg, this);

                    if (result != null)
                    {
                        ArgumentResults.Add(result);
                    }

                    if (error != null)
                    {
                        errors.Add(error);
                    }
                }
            }

            return errors;
        }

        [Obsolete]
        internal static (ArgumentResult, ParseError) Validate(
            Argument argument, 
            SymbolResult symbolResult)
        {
            ArgumentResult result = null;

            var error = symbolResult.UnrecognizedArgumentError(argument) ??
                        symbolResult.CustomError(argument);

            if (error == null)
            {
                result = Parse(symbolResult, argument);

                var canTokenBeRetried =
                    symbolResult.Symbol is ICommand ||
                    argument.Arity.MinimumNumberOfValues == 0;

                switch (result)
                {
                    case FailedArgumentArityResult arityFailure:

                        error = new ParseError(arityFailure.ErrorMessage,
                                               symbolResult,
                                               canTokenBeRetried);
                        break;

                    case FailedArgumentTypeConversionResult conversionFailure:

                        error = new ParseError(conversionFailure.ErrorMessage,
                                               symbolResult,
                                               canTokenBeRetried);
                        break;

                    case FailedArgumentResult general:

                        error = new ParseError(general.ErrorMessage,
                                               symbolResult,
                                               false);
                        break;
                }
            }

            return (result, error);
        }

        internal static ArgumentResult Parse(
            ArgumentResult2 argumentResult,
            IArgument argument) =>
            Parse(argumentResult.Parent, argument);

        internal static ArgumentResult Parse(
            SymbolResult symbolResult,
            IArgument argument)
        {
            if (ShouldCheckArity() &&
                     ArgumentArity.Validate(symbolResult,
                                            argument,
                                            argument.Arity.MinimumNumberOfValues,
                                            argument.Arity.MaximumNumberOfValues) is FailedArgumentResult failedResult)
            {
                return failedResult;
            }

            if (symbolResult.UseDefaultValueFor(argument))
            {
                var defaultValueFor = symbolResult.GetDefaultValueFor(argument);

                return ArgumentResult.Success(argument, defaultValueFor);
            }

            if (argument is Argument a &&
                a.ConvertArguments != null)
            {
                var success = a.ConvertArguments(symbolResult, out var value);

                if (value is ArgumentResult argumentResult)
                {
                    return argumentResult;
                }
                else if (success)
                {
                    return ArgumentResult.Success(argument, value);
                }
                else
                {
                    return ArgumentResult.Failure(argument, symbolResult.ErrorMessage ?? $"Invalid: {symbolResult.Token} {string.Join(" ", symbolResult.Arguments)}");
                }
            }

            switch (argument.Arity.MaximumNumberOfValues)
            {
                case 0:
                    return ArgumentResult.Success(argument, null);

                case 1:
                    return ArgumentResult.Success(argument, symbolResult.Arguments.SingleOrDefault());

                default:
                    return ArgumentResult.Success(argument, symbolResult.Arguments);
            }

            bool ShouldCheckArity()
            {
                return !(symbolResult is OptionResult optionResult) ||
                       !optionResult.IsImplicit;
            }
        }

        internal ParseError UnrecognizedArgumentError(Argument argument)
        {
            if (argument.AllowedValues?.Count > 0 &&
                Tokens.Count > 0)
            {
                foreach (var token in Tokens)
                {
                    if (!argument.AllowedValues.Contains(token.Value))
                    {
                        return new ParseError(
                            ValidationMessages
                                .UnrecognizedArgument(token.Value, argument.AllowedValues),
                            this,
                            canTokenBeRetried: false);
                    }
                }
            }

            return null;
        }

        internal ParseError CustomError(Argument argument)
        {
            foreach (var symbolValidator in argument.SymbolValidators)
            {
                var errorMessage = symbolValidator(this);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return new ParseError(errorMessage, this, false);
                }
            }

            return null;
        }
    }
}
