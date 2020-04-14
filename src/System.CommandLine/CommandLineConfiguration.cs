// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Collections;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public class CommandLineConfiguration
    {
        private IReadOnlyCollection<InvocationMiddleware>? _middlewarePipeline;
        private Func<BindingContext, IHelpBuilder>? _helpBuilderFactory;
        private readonly SymbolSet _symbols = new SymbolSet();

        public CommandLineConfiguration(
            IReadOnlyCollection<Symbol> symbols,
            IReadOnlyCollection<char>? argumentDelimiters = null,
            bool enablePosixBundling = true,
            bool enableDirectives = true,
            ValidationMessages? validationMessages = null,
            ResponseFileHandling responseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            IReadOnlyCollection<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, IHelpBuilder>? helpBuilderFactory = null)
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
                foreach (var alias in symbol.RawAliases)
                {
                    foreach (var delimiter in ArgumentDelimiters)
                    {
                        if (alias.Contains(delimiter))
                        {
                            throw new ArgumentException($"{symbol.GetType().Name} \"{alias}\" is not allowed to contain a delimiter but it contains \"{delimiter}\"");
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
                // reuse existing auto-generated root command, if one is present, to prevent repeated mutations
                rootCommand = symbols.SelectMany(s => s.Parents)
                                     .OfType<RootCommand>()
                                     .FirstOrDefault();

                if (rootCommand == null)
                {
                    rootCommand = new RootCommand();

                    foreach (var symbol in symbols)
                    {
                        rootCommand.Add(symbol);
                    }
                }

                RootCommand = rootCommand;
            }

            _symbols.Add(RootCommand);

            AddGlobalOptionsToChildren(rootCommand);

            EnablePosixBundling = enablePosixBundling;
            EnableDirectives = enableDirectives;
            ValidationMessages = validationMessages ?? ValidationMessages.Instance;
            ResponseFileHandling = responseFileHandling;
            _middlewarePipeline = middlewarePipeline;
            _helpBuilderFactory = helpBuilderFactory;
        }

        private void AddGlobalOptionsToChildren(Command parentCommand)
        {
            foreach (var globalOption in parentCommand.GlobalOptions)
            {
                foreach (var child in parentCommand.Children.FlattenBreadthFirst(c => c.Children))
                {
                    if (child is Command childCommand)
                    {
                        if (!childCommand.Children.IsAnyAliasInUse(globalOption, out _))
                        {
                            childCommand.AddOption(globalOption);
                        }
                    }
                }
            }
        }
        
        public ISymbolSet Symbols => _symbols;

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool EnableDirectives { get; }

        public bool EnablePosixBundling { get; }

        public ValidationMessages ValidationMessages { get; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory =>
            _helpBuilderFactory ??= context => new HelpBuilder(context.Console);

        internal IReadOnlyCollection<InvocationMiddleware> Middleware =>
            _middlewarePipeline ??= new List<InvocationMiddleware>();

        public ICommand RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
