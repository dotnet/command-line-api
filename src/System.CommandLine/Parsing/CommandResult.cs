// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// A result produced when parsing a <see cref="Command" />.
    /// </summary>
    public sealed class CommandResult : SymbolResult
    {
        private Dictionary<string, SymbolResult>? _namedResults;

        internal CommandResult(
            Command command,
            Token token,
            SymbolResultTree symbolResultTree,
            CommandResult? parent = null) :
            base(symbolResultTree, parent)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        /// <summary>
        /// The command to which the result applies.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The token that was parsed to specify the command.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// Child symbol results in the parse tree.
        /// </summary>
        public IEnumerable<SymbolResult> Children => SymbolResultTree.GetChildren(this);

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(CommandResult)}: {Command.Name} {string.Join(" ", Tokens.Select(t => t.Value))}";

        internal T? GetValue<T>(string name)
        {
            if (_namedResults is null)
            {
                Dictionary<string, SymbolResult> cache = new (StringComparer.Ordinal);

                foreach (KeyValuePair<Symbol, SymbolResult> pair in SymbolResultTree)
                {
                    if (ReferenceEquals(pair.Value.Parent, this))
                    {
                        cache.Add(pair.Key.Name, pair.Value);
                    }
                }

                _namedResults = cache;
            }

            _namedResults.TryGetValue(name, out SymbolResult? symbolResult);

            return symbolResult switch
            {
                ArgumentResult argumentResult => argumentResult.GetValueOrDefault<T>(),
                OptionResult optionResult => optionResult.GetValueOrDefault<T>(),
                _ => (T?)ArgumentConverter.GetDefaultValue(typeof(T))
            };
        }

        internal override bool UseDefaultValueFor(ArgumentResult argumentResult)
            => argumentResult.Argument.HasDefaultValue && argumentResult.Tokens.Count == 0;

        /// <param name="completeValidation">Only the inner most command goes through complete validation.</param>
        internal void Validate(bool completeValidation)
        {
            if (completeValidation)
            {
                if (Command.Handler is null && Command.HasSubcommands)
                {
                    SymbolResultTree.InsertFirstError(
                        new ParseError(LocalizationResources.RequiredCommandWasNotProvided(), this));
                }

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

                if (!completeValidation && !(option.AppliesToSelfAndChildren || option.Argument.HasDefaultValue || (option is HelpOption or VersionOption)))
                {
                    continue;
                }

                OptionResult optionResult;
                ArgumentResult argumentResult;

                if (!SymbolResultTree.TryGetValue(option, out SymbolResult? symbolResult))
                {
                    if (option.IsRequired)
                    {
                        AddError(LocalizationResources.RequiredOptionWasNotProvided(option.Name));
                        continue;
                    }
                    else if (option.Argument.HasDefaultValue)
                    {
                        optionResult = new(option, SymbolResultTree, null, this);
                        SymbolResultTree.Add(optionResult.Option, optionResult);

                        argumentResult = new(optionResult.Option.Argument, SymbolResultTree, optionResult);
                        SymbolResultTree.Add(optionResult.Option.Argument, argumentResult);
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
                        break;
                    }
                }

                _ = argumentResult.GetArgumentConversionResult();
            }
        }

        private void ValidateArguments(bool completeValidation)
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
                else if (argument.HasDefaultValue)
                {
                    argumentResult = new ArgumentResult(argument, SymbolResultTree, this);
                    SymbolResultTree[argument] = argumentResult;
                }
                else if (argument.Arity.MinimumNumberOfValues > 0)
                {
                    AddError(LocalizationResources.RequiredArgumentMissing(this));
                    continue;
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
