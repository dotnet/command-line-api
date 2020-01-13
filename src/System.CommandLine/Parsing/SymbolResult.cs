// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public abstract class SymbolResult
    {
        private protected readonly List<Token> _tokens = new List<Token>();
        private ArgumentConversionResultSet _results;
        private ValidationMessages _validationMessages;
        private readonly Dictionary<IArgument, object> _defaultArgumentValues = new Dictionary<IArgument, object>();

        private protected SymbolResult(
            ISymbol symbol, 
            Token token, 
            SymbolResult parent = null)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Token = token ?? throw new ArgumentNullException(nameof(token));

            Parent = parent;
        }

        internal virtual ArgumentConversionResult ArgumentConversionResult
        {
            get
            {
                var argument =  Symbol switch {
                    IOption o => o.Argument,
                    IArgument a => a,
                    _ => null
                };

                return ArgumentConversionResults.SingleOrDefault() ??
                       ArgumentConversionResult.None(argument);
            }
        }

        internal ArgumentConversionResultSet ArgumentConversionResults
        {
            get
            {
                if (_results == null)
                {
                    var results = Children
                                  .OfType<ArgumentResult>()
                                  .Select(r => ArgumentResult.Convert(r, r.Argument));

                    _results = new ArgumentConversionResultSet();

                    foreach (var result in results)
                    {
                        _results.Add(result);
                    }

                    return _results;
                }

                return _results;
            }
        }

        public string ErrorMessage { get; set; }

        public SymbolResultSet Children { get; } = new SymbolResultSet();

        public SymbolResult Parent { get; }

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

        internal object GetDefaultValueFor(IArgument argument)
        {
            return _defaultArgumentValues.GetOrAdd(
                argument,
                a => a.GetDefaultValue());
        }

        internal bool UseDefaultValueFor(IArgument argument)
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

            return _defaultArgumentValues.ContainsKey(argument);
        }

        public override string ToString() => $"{GetType().Name}: {Token}";

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
                            this);
                    }
                }
            }

            return null;
        }
    }
}
