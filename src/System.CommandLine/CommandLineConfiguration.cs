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
            bool enablePosixBundling = true,
            bool enablePositionalOptions = false,
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

            Symbols.Add(RootCommand);

            EnablePosixBundling = enablePosixBundling;
            EnablePositionalOptions = enablePositionalOptions;
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

        public SymbolSet Symbols { get; } = new SymbolSet();

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool EnablePositionalOptions { get; private set; }

        public bool EnablePosixBundling { get; }

        public ValidationMessages ValidationMessages { get; }

        internal IReadOnlyCollection<InvocationMiddleware> Middleware =>
            _middlewarePipeline ??
            (_middlewarePipeline = new List<InvocationMiddleware>());

        internal Command RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
