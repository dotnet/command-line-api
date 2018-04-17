using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public abstract class ParsedSymbol
    {
        private readonly Lazy<string> defaultValue;
        private readonly Func<object> materialize;
        protected internal readonly List<string> arguments = new List<string>();

        private bool considerAcceptingAnotherArgument = true;

        protected internal ParsedSymbol(Symbol symbol, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Token = token;

            defaultValue = new Lazy<string>(Symbol.ArgumentsRule.GetDefaultValue);

            materialize = () => Symbol.ArgumentsRule.Materialize(this);
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

        public Symbol Symbol { get; }

        public string Token { get; }

        public IReadOnlyCollection<string> Aliases => Symbol.Aliases;

        public bool HasAlias(string alias) => Symbol.HasAlias(alias);

        internal OptionError Validate()
        {
            var error = Symbol.Validate(this);
            return string.IsNullOrWhiteSpace(error)
                       ? null
                       : new OptionError(error, Token, this);
        }

        internal void OptionWasRespecified() => considerAcceptingAnotherArgument = true;

        public ParsedSymbol TryTakeToken(Token token)
        {
            var option = TryTakeArgument(token) ??
                         TryTakeOptionOrCommand(token);
            considerAcceptingAnotherArgument = false;
            return option;
        }

        private ParsedSymbol TryTakeArgument(Token token)
        {
            if (token.Type != TokenType.Argument)
            {
                return null;
            }

            if (!considerAcceptingAnotherArgument &&
                !Symbol.IsCommand)
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

        protected ParsedSymbol TryTakeOptionOrCommand(Token token)
        {
            var child = Children
                .SingleOrDefault(o =>
                                     o.Symbol.DefinedSymbols
                                      .Any(oo => oo.RawAliases.Contains(token.Value)));

            if (child != null)
            {
                return child.TryTakeToken(token);
            }

            if (token.Type == TokenType.Command &&
                Children.Any(o => o.Symbol.IsCommand && !o.HasAlias(token.Value)))
            {
                // if a subcommand has already been applied, don't accept this one
                return null;
            }

            var applied =
                Children.SingleOrDefault(o => o.Symbol.HasRawAlias(token.Value));

            if (applied != null)
            {
                applied.OptionWasRespecified();
                return applied;
            }

            applied =
                Symbol.DefinedSymbols
                      .Where(o => o.RawAliases.Contains(token.Value))
                      .Select(o => Create(o, token.Value))
                      .SingleOrDefault();

            if (applied != null)
            {
                Children.Add(applied);
            }

            return applied;
        }

        public override string ToString() => this.Diagram();

        internal static ParsedSymbol Create(Symbol symbol, string token)
        {
            switch (symbol)
            {
                case Command command:
                    return new ParsedCommand(command);

                case Option option:
                    return new ParsedOption(option, token);

                default: 
                    throw new ArgumentException($"Unrecognized symbol type: {symbol.GetType()}");
            }
        }

        
        public object Value()
        {
            try
            {
                return materialize();
            }
            catch (Exception exception)
            {
                var argumentsDescription = Arguments.Any()
                                               ? string.Join(", ", Arguments)
                                               : " (none)";
                throw new ParseException(
                    $"An exception occurred while getting the value for option '{Symbol.Name}' based on argument(s): {argumentsDescription}.",
                    exception);
            }
        }
    }
}
