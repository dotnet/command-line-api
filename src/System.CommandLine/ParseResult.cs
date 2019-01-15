// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ParseResult
    {
        private readonly List<ParseError> _errors = new List<ParseError>();

        internal ParseResult(
            CommandResult rootCommandResult,
            CommandResult commandResult,
            IDirectiveCollection directives,
            IReadOnlyCollection<string> tokens,
            IReadOnlyCollection<string> unparsedTokens,
            IReadOnlyCollection<string> unmatchedTokens,
            IReadOnlyCollection<ParseError> errors,
            string rawInput)
        {
            RootCommandResult = rootCommandResult;
            CommandResult = commandResult;
            Directives = directives;
            Tokens = tokens;
            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;
            RawInput = rawInput;

            if (errors != null)
            {
                _errors.AddRange(errors);
            }

            AddImplicitOptionsAndCheckForErrors();
        }

        public CommandResult CommandResult { get; }

        public CommandResult RootCommandResult { get; }

        public IReadOnlyCollection<ParseError> Errors => _errors;

        public IDirectiveCollection Directives { get; }

        public IReadOnlyCollection<string> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        internal string RawInput { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        private void AddImplicitOptionsAndCheckForErrors()
        {
            foreach (var result in RootCommandResult.AllSymbolResults().ToArray())
            {
                if (result is CommandResult command)
                {
                    foreach (var symbol in command.Command.Children)
                    {
                        if (symbol.Argument != null &&
                            symbol.Argument.HasDefaultValue &&
                            command.Children[symbol.Name] == null)
                        {
                            switch (symbol)
                            {
                                case IOption option:
                                    command.AddImplicitOption(option);
                                    break;
                            }
                        }
                    }


                    // *** Please review. Test for null is in response to a bug. Less confident on check of Command.Arguments ***
                    if (command.Command.Argument != null &&
                        command.Command.Argument.HasDefaultValue &&
                        command.Arguments.Count == 0)
                    {
                        var defaultValue = command.Command.Argument.GetDefaultValue();

                        if (defaultValue is string stringArg)
                        {
                            command.TryTakeToken(new Token(stringArg, TokenType.Argument));
                        }
                        else
                        {
                            command.UseDefaultValue = true;
                        }
                    }
                }

                var error = result.Validate();

                if (error != null)
                {
                    _errors.Add(error);
                }
            }

            if (CommandResult.Command is Command cmd &&
                cmd.Handler == null && 
                cmd.Children.OfType<ICommand>().Any())
            {
                _errors.Insert(0,
                               new ParseError(
                                   CommandResult.ValidationMessages.RequiredCommandWasNotProvided(),
                                   CommandResult));
            }
        }

        public object ValueForOption(
            string alias) =>
            ValueForOption<object>(alias);

        public T ValueForOption<T>(
            string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            return this[alias].GetValueOrDefault<T>();
        }

        public SymbolResult this[string alias] => CommandResult.Children[alias];

        public override string ToString() => $"{nameof(ParseResult)}: {this.Diagram()}";
    }
}
