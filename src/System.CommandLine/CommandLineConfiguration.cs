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
        private readonly IReadOnlyCollection<InvocationMiddleware> _middlewarePipeline;
        private readonly Func<BindingContext, IHelpBuilder> _helpBuilderFactory;
        private readonly SymbolSet _symbols = new SymbolSet();

        public CommandLineConfiguration(
            IReadOnlyCollection<Symbol> symbols,
            IReadOnlyList<char>? argumentDelimiters = null,
            bool enablePosixBundling = true,
            bool enableDirectives = true,
            ValidationMessages? validationMessages = null,
            ResponseFileHandling responseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            IReadOnlyCollection<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, IHelpBuilder>? helpBuilderFactory = null)
        {
            if (symbols is null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            if (!symbols.Any())
            {
                throw new ArgumentException("You must specify at least one option or command.");
            }

            if (argumentDelimiters is null)
            {
                ArgumentDelimitersInternal = new []
                {
                    ':',
                    '='
                };
            }
            else
            {
                ArgumentDelimitersInternal = argumentDelimiters;
            }

            foreach (var symbol in symbols)
            {
                if (symbol is IIdentifierSymbol optionOrCommand)
                {
                    foreach (var alias in optionOrCommand.Aliases)
                    {
                        for (var i = 0; i < ArgumentDelimiters.Count; i++)
                        {
                            var delimiter = ArgumentDelimiters[i];
                            if (alias.Contains(delimiter))
                            {
                                throw new ArgumentException($"{symbol.GetType().Name} \"{alias}\" is not allowed to contain a delimiter but it contains \"{delimiter}\"");
                            }
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
                RootCommand? parentRootCommand = 
                    symbols.SelectMany(s => s.Parents)
                           .OfType<RootCommand>()
                           .FirstOrDefault();

                if (parentRootCommand is null)
                {
                    parentRootCommand = new RootCommand();

                    foreach (var symbol in symbols)
                    {
                        parentRootCommand.Add(symbol);
                    }
                }

                RootCommand = rootCommand = parentRootCommand;
            }

            _symbols.Add(RootCommand);

            AddGlobalOptionsToChildren(rootCommand);

            EnablePosixBundling = enablePosixBundling;
            EnableDirectives = enableDirectives;
            ValidationMessages = validationMessages ?? ValidationMessages.Instance;
            ResponseFileHandling = responseFileHandling;
            _middlewarePipeline = middlewarePipeline ?? new List<InvocationMiddleware>();
            _helpBuilderFactory = helpBuilderFactory ?? (context => new HelpBuilder(context.Console));
        }

        private void AddGlobalOptionsToChildren(Command parentCommand)
        {
            foreach (var child in parentCommand.Children.FlattenBreadthFirst(c => c.Children))
            {
                if (child is Command childCommand)
                {
                    foreach (var globalOption in parentCommand.GlobalOptions)
                    {
                        if (!childCommand.Children.IsAnyAliasInUse(globalOption, out _))
                        {
                            childCommand.AddOption(globalOption);
                        }
                    }

                    AddGlobalOptionsToChildren(childCommand);
                }
            }
        }

        public ISymbolSet Symbols => _symbols;

        public IReadOnlyList<char> ArgumentDelimiters => ArgumentDelimitersInternal;

        internal IReadOnlyList<char> ArgumentDelimitersInternal { get; }
     
        public bool EnableDirectives { get; }

        public bool EnablePosixBundling { get; }

        public ValidationMessages ValidationMessages { get; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory => _helpBuilderFactory;

        internal IReadOnlyCollection<InvocationMiddleware> Middleware => _middlewarePipeline;

        public ICommand RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
