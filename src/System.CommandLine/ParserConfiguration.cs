using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ParserConfiguration
    {
        public ParserConfiguration(
            IReadOnlyCollection<SymbolDefinition> symbolDefinitions,
            IReadOnlyCollection<char> argumentDelimiters = null,
            IReadOnlyCollection<string> prefixes = null,
            bool allowUnbundling = true,
            IValidationMessages validationMessages = null,
            ResponseFileHandling responseFileHandling = default(ResponseFileHandling) )
        {
            if (symbolDefinitions == null)
            {
                throw new ArgumentNullException(nameof(symbolDefinitions));
            }

            if (!symbolDefinitions.Any())
            {
                throw new ArgumentException("You must specify at least one option.");
            }

            if (!symbolDefinitions.OfType<CommandDefinition>().Any())
            {
                var symbolsDefinition = symbolDefinitions.ToArray();
                RootCommandDefinition = CommandDefinition.CreateImplicitRootCommand(symbolsDefinition);
                SymbolDefinitions.Add(RootCommandDefinition);
            }
            else
            {
                SymbolDefinitions.AddRange(symbolDefinitions);
            }

            ArgumentDelimiters = argumentDelimiters ?? new[] { ':', '=' };
            AllowUnbundling = allowUnbundling;
            ValidationMessages = validationMessages ?? new DefaultValidationMessages();
            ResponseFileHandling = responseFileHandling;

            if (prefixes?.Count > 0)
            {
                foreach (SymbolDefinition symbol in symbolDefinitions)
                {
                    foreach (string alias in symbol.RawAliases.ToList())
                    {
                        if (!prefixes.All(prefix => alias.StartsWith(prefix)))
                        {
                            foreach (var prefix in prefixes)
                            {
                                symbol.AddAlias(prefix + alias);
                            }
                        }
                    }
                }
            }
        }

        public SymbolDefinitionSet SymbolDefinitions { get; } = new SymbolDefinitionSet();

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool AllowUnbundling { get; }
        public IValidationMessages ValidationMessages { get; }
        internal CommandDefinition RootCommandDefinition { get; }

        internal bool RootCommandIsImplicit => RootCommandDefinition != null;

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
