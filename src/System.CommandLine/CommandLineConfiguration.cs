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
            IReadOnlyCollection<Symbol> symbols,
            IReadOnlyCollection<char> argumentDelimiters = null,
            IReadOnlyCollection<string> prefixes = null,
            bool allowUnbundling = true,
            ValidationMessages validationMessages = null,
            ResponseFileHandling responseFileHandling = default(ResponseFileHandling),
            IReadOnlyCollection<InvocationMiddleware> middlewarePipeline = null)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            if (!symbols.Any())
            {
                throw new ArgumentException("You must specify at least one option.");
            }

            ArgumentDelimiters = argumentDelimiters ?? new[] { ':', '=', ' ' };

            foreach (var symbol in symbols)
            {
                foreach (var alias in symbol.RawAliases)
                {
                    foreach (var delimiter in ArgumentDelimiters)
                    {
                        if (alias.Contains(delimiter))
                        {
                            throw new ArgumentException($"Symbol cannot contain delimiter: \"{delimiter}\"");
                        }
                    }
                }
            }

            if (symbols.Count == 1 &&
                symbols.Single() is Command rootCommand)
            {
                RootCommand = rootCommand;
            }
            else
            {
                RootCommand = new Command(
                    CommandLineBuilder.ExeName,
                    "",
                    symbols);
            }

            Symbol.Add(RootCommand);

            AllowUnbundling = allowUnbundling;
            ValidationMessages = validationMessages ?? ValidationMessages.Instance;
            ResponseFileHandling = responseFileHandling;
            _middlewarePipeline = middlewarePipeline;
            Prefixes = prefixes;

            if (prefixes?.Count > 0)
            {
                foreach (var symbol in symbols)
                {
                    foreach (var alias in symbol.RawAliases.ToList())
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

        public SymbolSet Symbol { get; } = new SymbolSet();

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool AllowUnbundling { get; }

        public ValidationMessages ValidationMessages { get; }

        internal IReadOnlyCollection<InvocationMiddleware> InvocationList =>
            _middlewarePipeline ??
            (_middlewarePipeline = new List<InvocationMiddleware>());

        internal Command RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
