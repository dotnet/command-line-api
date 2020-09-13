// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Collections;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public class Command : 
        Symbol, 
        ICommand, 
        IEnumerable<Symbol>
    {
        private readonly SymbolSet _globalOptions = new SymbolSet();

        public Command(string name, string? description = null) : base(new[] { name }, description)
        {
            Name = name;
        }

        public IEnumerable<Argument> Arguments => 
            Children.OfType<Argument>();

        public IEnumerable<Option> Options =>
            Children.OfType<Option>()
                    .Concat(Parents
                            .OfType<Command>()
                            .SelectMany(c => c.GlobalOptions));

        public IEnumerable<Option> GlobalOptions => _globalOptions.OfType<Option>();

        public void AddArgument(Argument argument) => AddArgumentInner(argument);

        public void AddCommand(Command command) => AddSymbol(command);

        public void AddOption(Option option) => AddSymbol(option);

        public void AddGlobalOption(Option option)
        {
            _globalOptions.Add(option);
            Children.AddWithoutAliasCollisionCheck(option);
        }

        public bool TryAddGlobalOption(Option option)
        {
            if (!_globalOptions.IsAnyAliasInUse(option, out _))
            {
                _globalOptions.Add(option);
                Children.AddWithoutAliasCollisionCheck(option);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Add(Symbol symbol) => AddSymbol(symbol);

        public void Add(Argument argument) => AddArgument(argument);

        private protected override void AddSymbol(Symbol symbol)
        {
            if (symbol is IOption option)
            {
                _globalOptions.ThrowIfAnyAliasIsInUse(option);
            }

            symbol.AddParent(this);

            base.AddSymbol(symbol);
        }

        internal List<ValidateSymbol<CommandResult>> Validators { get; } = new List<ValidateSymbol<CommandResult>>();

        public void AddValidator(ValidateSymbol<CommandResult> validate) => Validators.Add(validate);

        public bool TreatUnmatchedTokensAsErrors { get; set; } = true;

        public ICommandHandler? Handler { get; set; }

        public IEnumerator<Symbol> GetEnumerator() => Children.OfType<Symbol>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerable<IArgument> ICommand.Arguments => Arguments;

        IEnumerable<IOption> ICommand.Options => Options;

        internal Parser? ImplicitParser { get; set; }

        private protected override void ChooseNameForUnnamedArgument(Argument argument)
        {
            argument.Name = argument.ArgumentType.Name.ToLower();
        }
    }
}
