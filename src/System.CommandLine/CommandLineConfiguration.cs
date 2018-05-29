using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine
{
    public class CommandLineConfiguration
    {
        private IReadOnlyCollection<InvocationMiddleware> _middlewarePipeline;

        public CommandLineConfiguration(
            IReadOnlyCollection<SymbolDefinition> symbolDefinitions,
            IReadOnlyCollection<char> argumentDelimiters = null,
            IReadOnlyCollection<string> prefixes = null,
            bool allowUnbundling = true,
            ValidationMessages validationMessages = null,
            ResponseFileHandling responseFileHandling = default(ResponseFileHandling),
            IReadOnlyCollection<InvocationMiddleware> middlewarePipeline = null)
        {
            if (symbolDefinitions == null)
            {
                throw new ArgumentNullException(nameof(symbolDefinitions));
            }

            if (!symbolDefinitions.Any())
            {
                throw new ArgumentException("You must specify at least one option.");
            }

            ArgumentDelimiters = argumentDelimiters ?? new[] { ':', '=' };

            foreach (var definition in symbolDefinitions)
            {
                foreach (var alias in definition.RawAliases)
                {
                    foreach (var delimiter in ArgumentDelimiters)
                    {
                        if (alias.Contains(delimiter))
                        {
                            throw new ArgumentException($"Symbol cannot contain delimiter: {delimiter}");
                        }
                    }
                }
            }

            if (symbolDefinitions.Count == 1 &&
                symbolDefinitions.Single() is CommandDefinition rootComanCommandDefinition)
            {
                RootCommandDefinition = rootComanCommandDefinition;
            }
            else
            {
                RootCommandDefinition = new CommandDefinition(
                    CommandLineBuilder.ExeName,
                    "",
                    symbolDefinitions);
            }

            SymbolDefinitions.Add(RootCommandDefinition);

            AllowUnbundling = allowUnbundling;
            ValidationMessages = validationMessages ?? ValidationMessages.Instance;
            ResponseFileHandling = responseFileHandling;
            _middlewarePipeline = middlewarePipeline;
            Prefixes = prefixes;

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

        public IReadOnlyCollection<string> Prefixes { get; }

        public SymbolDefinitionSet SymbolDefinitions { get; } = new SymbolDefinitionSet();

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool AllowUnbundling { get; }

        public ValidationMessages ValidationMessages { get; }

        internal IReadOnlyCollection<InvocationMiddleware> InvocationList =>
            _middlewarePipeline ??
            (_middlewarePipeline = new List<InvocationMiddleware>());

        internal CommandDefinition RootCommandDefinition { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
