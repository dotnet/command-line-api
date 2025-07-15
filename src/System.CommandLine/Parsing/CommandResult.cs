// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing a <see cref="Command" />.
    /// </summary>
    public sealed class CommandResult : SymbolResult
    {
        internal CommandResult(
            Command command,
            Token token,
            SymbolResultTree symbolResultTree,
            CommandResult? parent = null) :
            base(symbolResultTree, parent)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            IdentifierToken = token ?? throw new ArgumentNullException(nameof(token));
        }

        /// <summary>
        /// The command to which the result applies.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The token that was parsed to specify the command.
        /// </summary>
        public Token IdentifierToken { get; }

        /// <summary>
        /// Child symbol results in the parse tree.
        /// </summary>
        public IEnumerable<SymbolResult> Children => SymbolResultTree.GetChildren(this);

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(CommandResult)}: {IdentifierToken.Value} {string.Join(" ", Tokens.Select(t => t.Value))}";

        internal override bool UseDefaultValueFor(ArgumentResult argumentResult)
            => argumentResult.Argument.HasDefaultValue && argumentResult.Tokens.Count == 0;

        internal void Validate(bool isInnermostCommand)
        {
            if (isInnermostCommand)
            {
                if (Command.HasValidators)
                {
                    int errorCountBefore = SymbolResultTree.ErrorCount;
                    for (var i = 0; i < Command.Validators.Count; i++)
                    {
                        Command.Validators[i](this);
                    }

                    if (SymbolResultTree.ErrorCount != errorCountBefore)
                    {
                        return;
                    }
                }
            }

            if (Command.HasOptions)
            {
                ValidateOptionsAndAddDefaultResults(isInnermostCommand);
            }

            if (Command.HasArguments)
            {
                ValidateArgumentsAndAddDefaultResults(isInnermostCommand);
            }
        }

        private void ValidateOptionsAndAddDefaultResults(bool completeValidation)
        {
            var options = Command.Options;
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];

                if (!completeValidation && !(option.Recursive || option.Argument.HasDefaultValue || option is VersionOption))
                {
                    continue;
                }

                OptionResult optionResult;
                ArgumentResult argumentResult;

                if (!SymbolResultTree.TryGetValue(option, out SymbolResult? symbolResult))
                {
                    if (option.Required || option.Argument.HasDefaultValue)
                    {
                        optionResult = new(option, SymbolResultTree, null, this);
                        SymbolResultTree.Add(optionResult.Option, optionResult);

                        argumentResult = new(optionResult.Option.Argument, SymbolResultTree, optionResult);
                        SymbolResultTree.Add(optionResult.Option.Argument, argumentResult);

                        if (option is { Required: true, Argument.HasDefaultValue: false })
                        {
                            argumentResult.AddError(LocalizationResources.RequiredOptionWasNotProvided(option.Name));
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    optionResult = (OptionResult)symbolResult;
                    argumentResult = (ArgumentResult)SymbolResultTree[option.Argument];
                }

                // When_there_is_an_arity_error_then_further_errors_are_not_reported
                if (!ArgumentArity.Validate(argumentResult, out var error))
                {
                    optionResult.AddError(error.ErrorMessage!);
                    continue;
                }

                if (optionResult.Option.HasValidators)
                {
                    int errorsBefore = SymbolResultTree.ErrorCount;

                    for (var j = 0; j < optionResult.Option.Validators.Count; j++)
                    {
                        optionResult.Option.Validators[j](optionResult);
                    }

                    if (errorsBefore != SymbolResultTree.ErrorCount)
                    {
                        continue;
                    }
                }

                _ = argumentResult.GetArgumentConversionResult();
            }
        }

        private void ValidateArgumentsAndAddDefaultResults(bool completeValidation)
        {
            var arguments = Command.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                Argument argument = arguments[i];

                if (!completeValidation && !argument.HasDefaultValue)
                {
                    continue;
                }

                ArgumentResult? argumentResult;
                if (SymbolResultTree.TryGetValue(argument, out SymbolResult? symbolResult))
                {
                    argumentResult = (ArgumentResult)symbolResult;
                }
                else if (argument.HasDefaultValue || argument.Arity.MinimumNumberOfValues > 0)
                {
                    argumentResult = new ArgumentResult(argument, SymbolResultTree, this);
                    SymbolResultTree[argument] = argumentResult;

                    if (!argument.HasDefaultValue && argument.Arity.MinimumNumberOfValues > 0)
                    {
                        argumentResult.AddError(LocalizationResources.RequiredArgumentMissing(argumentResult));
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                _ = argumentResult.GetArgumentConversionResult();
            }
        }
    }
}
