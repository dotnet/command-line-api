// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing a <see cref="Command" />.
    /// </summary>
    internal sealed class CliCommandResultInternal
        : CliSymbolResultInternal
    {
        internal CliCommandResultInternal(
            CliCommand command,
            CliToken token,
            SymbolResultTree symbolResultTree,
            CliCommandResultInternal? parent = null) :
            base(symbolResultTree, parent)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            IdentifierToken = token ?? throw new ArgumentNullException(nameof(token));
        }

        /// <summary>
        /// The command to which the result applies.
        /// </summary>
        public CliCommand Command { get; }

        // FIXME: should CliToken be public or internal?
        /// <summary>
        /// The token that was parsed to specify the command.
        /// </summary>
        internal CliToken IdentifierToken { get; }

        /// <summary>
        /// Child symbol results in the parse tree.
        /// </summary>
        public IEnumerable<CliSymbolResultInternal> Children => SymbolResultTree.GetChildren(this);

        private CliCommandResult? commandResult;
        public CliCommandResult CommandResult
        {
            get
            {
                if (commandResult is null)
                {
                    var parent = Parent is CliCommandResultInternal commandResultInternal
                        ? commandResultInternal.CommandResult
                        : null;
                    commandResult = new CliCommandResult(Command, Tokens.Select(t => t.Location), parent);
                }
                // Reset unless we put tests in place to ensure it is not called in error handling before SymbolTree processing is complete
                commandResult.ValueResults = Children.Select(GetValueResult).OfType<CliValueResult>().ToList();
                return commandResult;
            }
        }

        private CliValueResult? GetValueResult(CliSymbolResultInternal symbolResult)
            => symbolResult switch
            {
                CliArgumentResultInternal argumentResult => argumentResult.ValueResult,
                CliOptionResultInternal optionResult => optionResult.ValueResult,
                _ => null!
            };

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(CliCommandResultInternal)}: {IdentifierToken.Value} {string.Join(" ", Tokens.Select(t => t.Value))}";

        // TODO: DefaultValues
        /*
        internal override bool UseDefaultValueFor(ArgumentResult argumentResult)
            => argumentResult.Argument.HasDefaultValue && argumentResult.Tokens.Count == 0;
        */

        // TODO: Validation
        /*
        /// <param name="completeValidation">Only the inner most command goes through complete validation.</param>
        internal void Validate(bool completeValidation)
        {
            if (completeValidation)
            {
                if (Command.HasSubcommands)
                {
                    SymbolResultTree.InsertFirstError(
                        new ParseError(LocalizationResources.RequiredCommandWasNotProvided(), this));
                }

                // TODO: validators
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

            // TODO: Validation
            if (Command.HasOptions)
            {
                ValidateOptions(completeValidation);
            }

            if (Command.HasArguments)
            {
                ValidateArguments(completeValidation);
            }
        }

        private void ValidateOptions(bool completeValidation)
        {
            var options = Command.Options;
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];

                // TODO: VersionOption, recursive options
                // if (!completeValidation && !(option.Recursive || option.Argument.HasDefaultValue || option is VersionOption))
                if (!completeValidation && !option.Argument.HasDefaultValue)
                {
                    continue;
                }

                CliOptionResultInternal optionResult;
                CliArgumentResultInternal argumentResult;

                if (!SymbolResultTree.TryGetValue(option, out CliSymbolResultInternal? symbolResult))
                {
                    if (option.Required || option.Argument.HasDefaultValue)
                    {
                        optionResult = new(option, SymbolResultTree, null, this);
                        SymbolResultTree.Add(optionResult.Option, optionResult);

                        argumentResult = new(optionResult.Option.Argument, SymbolResultTree, optionResult);
                        SymbolResultTree.Add(optionResult.Option.Argument, argumentResult);

                        if (option.Required && !option.Argument.HasDefaultValue)
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
                    optionResult = (CliOptionResultInternal)symbolResult;
                    argumentResult = (CliArgumentResultInternal)SymbolResultTree[option.Argument];
                }

                // When_there_is_an_arity_error_then_further_errors_are_not_reported
                if (!ArgumentArity.Validate(argumentResult, out var error))
                {
                    optionResult.AddError(error.ErrorMessage!);
                    continue;
                }

                // TODO: validators
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

                // TODO: Ensure all argument conversions are run for entered values
                _ = argumentResult.GetArgumentConversionResult();
            }
        }

        // TODO: Validation
        private void ValidateArguments(bool completeValidation)
        {
            var arguments = Command.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                CliArgument argument = arguments[i];

                if (!completeValidation && !argument.HasDefaultValue)
                {
                    continue;
                }

                CliArgumentResultInternal? argumentResult;
                if (SymbolResultTree.TryGetValue(argument, out CliSymbolResultInternal? symbolResult))
                {
                    argumentResult = (CliArgumentResultInternal)symbolResult;
                }
                else if (argument.HasDefaultValue || argument.Arity.MinimumNumberOfValues > 0)
                {
                    argumentResult = new CliArgumentResultInternal(argument, SymbolResultTree, this);
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
        */
    }
}
