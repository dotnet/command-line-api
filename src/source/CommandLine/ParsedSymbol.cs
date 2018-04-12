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

        protected internal ParsedSymbol(Option option, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            Option = option ?? throw new ArgumentNullException(nameof(option));

            Token = token;

            defaultValue = new Lazy<string>(Option.ArgumentsRule.GetDefaultValue);

            materialize = () => Option.ArgumentsRule.Materialize(this);
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

        public ParsedSymbolSet ParsedOptions { get; } = new ParsedSymbolSet();

        public string Name => Option.Name;

        public Option Option { get; }

        public string Token { get; }

        public IReadOnlyCollection<string> Aliases => Option.Aliases;

        public bool HasAlias(string alias) => Option.HasAlias(alias);

        internal OptionError Validate()
        {
            var error = Option.Validate(this);
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
                !Option.IsCommand)
            {
                // Options must be respecified in order to accept additional arguments. This is 
                // not the case for commands.
                return null;
            }

            foreach (var option in ParsedOptions)
            {
                var a = option.TryTakeToken(token);
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
            var childOption = ParsedOptions
                .SingleOrDefault(o =>
                                     o.Option.DefinedOptions
                                      .Any(oo => oo.RawAliases.Contains(token.Value)));

            if (childOption != null)
            {
                return childOption.TryTakeToken(token);
            }

            if (token.Type == TokenType.Command &&
                ParsedOptions.Any(o => o.Option.IsCommand && !o.HasAlias(token.Value)))
            {
                // if a subcommand has already been applied, don't accept this one
                return null;
            }

            var applied =
                ParsedOptions.SingleOrDefault(o => o.Option.HasRawAlias(token.Value));

            if (applied != null)
            {
                applied.OptionWasRespecified();
                return applied;
            }

            applied =
                Option.DefinedOptions
                      .Where(o => o.RawAliases.Contains(token.Value))
                      .Select(o => Create(o, token.Value))
                      .SingleOrDefault();

            if (applied != null)
            {
                ParsedOptions.Add(applied);
            }

            return applied;
        }

        public override string ToString() => this.Diagram();

        internal static ParsedSymbol Create(Option option, string token)
        {
            switch (option)
            {
                case Command command:
                    return new ParsedCommand(command);

                default:
                    return new ParsedOption(option, token);
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
                    $"An exception occurred while getting the value for option '{Option.Name}' based on argument(s): {argumentsDescription}.",
                    exception);
            }
        }
    }
}
