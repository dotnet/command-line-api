// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine
{
    public class CommandLineConfiguration
    {
        private IReadOnlyCollection<InvocationMiddleware> _middlewarePipeline;
        private Func<BindingContext, IHelpBuilder> _helpBuilderFactory;
        private readonly SymbolSet _symbols = new SymbolSet();

        public CommandLineConfiguration(
            IReadOnlyCollection<Symbol> symbols,
            IReadOnlyCollection<char> argumentDelimiters = null,
            IReadOnlyCollection<string> prefixes = null,
            bool enablePosixBundling = true,
            bool enablePositionalOptions = false,
            ValidationMessages validationMessages = null,
            ResponseFileHandling responseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            IReadOnlyCollection<InvocationMiddleware> middlewarePipeline = null,
            Func<BindingContext, IHelpBuilder> helpBuilderFactory = null)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            if (!symbols.Any())
            {
                throw new ArgumentException("You must specify at least one option or command.");
            }

            ArgumentDelimiters = argumentDelimiters ?? new[] { ':', '=' };

            foreach (var symbol in symbols)
            {
                foreach (var childSymbol in symbol.Children.FlattenBreadthFirst(o => o.Children))
                {
                    if (childSymbol.Argument.Arity.MaximumNumberOfArguments != 0 && string.IsNullOrEmpty(childSymbol.Argument.Name))
                    {
                        throw new ArgumentException(
                            ValidationMessages.RequiredArgumentNameMissing(childSymbol.Aliases.FirstOrDefault()));
                    }
                }

                foreach (var alias in symbol.RawAliases)
                {
                    foreach (var delimiter in ArgumentDelimiters)
                    {
                        if (alias.Contains(delimiter))
                        {
                            throw new SymbolCannotContainDelimiterArgumentException(delimiter);
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
                RootCommand = new RootCommand(symbols: symbols);
            }

            _symbols.Add(RootCommand);

            EnablePosixBundling = enablePosixBundling;
            EnablePositionalOptions = enablePositionalOptions;
            ValidationMessages = validationMessages ?? ValidationMessages.Instance;
            ResponseFileHandling = responseFileHandling;
            _middlewarePipeline = middlewarePipeline;
            _helpBuilderFactory = helpBuilderFactory;
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

        public ISymbolSet Symbols => _symbols;

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool EnablePositionalOptions { get; }

        public bool EnablePosixBundling { get; }

        public ValidationMessages ValidationMessages { get; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory =>
            _helpBuilderFactory ??
            (_helpBuilderFactory = context => new HelpBuilder(context.Console));

        internal IReadOnlyCollection<InvocationMiddleware> Middleware =>
            _middlewarePipeline ??
            (_middlewarePipeline = new List<InvocationMiddleware>());

        public ICommand RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
