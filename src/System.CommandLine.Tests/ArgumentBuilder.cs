using System.Collections.Generic;

namespace System.CommandLine.Tests
{
    public class ArgumentBuilder
    {
        private readonly List<Action<Argument>> _configureActions = new List<Action<Argument>>();

        internal List<ValidateSymbol> SymbolValidators { get; set; } = new List<ValidateSymbol>();

        public void Configure(Action<Argument> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _configureActions.Add(action);
        }

        public Argument Build()
        {
            var argument = new Argument(SymbolValidators);

            foreach (var configure in _configureActions)
            {
                configure(argument);
            }

            return argument;
        }
    }
}