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
    /// <summary>
    /// Represents the configuration used by the <see cref="Parser"/>.
    /// </summary>
    public class CommandLineConfiguration
    {
        private readonly IReadOnlyCollection<InvocationMiddleware> _middlewarePipeline;
        private readonly Func<BindingContext, IHelpBuilder> _helpBuilderFactory;
        private readonly SymbolSet _symbols = new SymbolSet();

        /// <summary>
        /// Initializes a new instance of the CommandLineConfiguration class.
        /// </summary>
        /// <param name="symbols">The symbols to parse.</param>
        /// <param name="argumentDelimiters">The characters used to delimit an option from its argument. In addition to
        /// one or more spaces, the default delimiters include <c>:</c> and <c>=</c>.</param>
        /// <param name="enablePosixBundling"><c>true</c> to enable POSIX bundling; otherwise, <c>false</c>.</param>
        /// <param name="enableDirectives"><c>true</c> to enable directive parsing; otherwise, <c>false</c>.</param>
        /// <param name="validationMessages">Provide custom validation messages.</param>
        /// <param name="responseFileHandling">One of the enumeration values that specifies how response files (.rsp) are handled.</param>
        /// <param name="middlewarePipeline">Provide a custom middleware pipeline.</param>
        /// <param name="helpBuilderFactory">Provide a custom help builder.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="symbols"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="symbols"/> does not contain at least one option or command.</exception>
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
                ArgumentDelimitersInternal = argumentDelimiters.Distinct().ToArray();
            }

            foreach (var symbol in symbols)
            {
                if (symbol is IIdentifierSymbol identifier)
                {
                    foreach (var alias in identifier.Aliases)
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
                // Reuse existing auto-generated root command, if one is present, to prevent repeated mutations
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

        /// <summary>
        /// Represents all of the symbols to parse.
        /// </summary>
        public ISymbolSet Symbols => _symbols;

        /// <summary>
        /// Represents all of the argument delimiters.
        /// </summary>
        public IReadOnlyList<char> ArgumentDelimiters => ArgumentDelimitersInternal;

        internal IReadOnlyList<char> ArgumentDelimitersInternal { get; }
     
        /// <summary>
        /// Gets whether directives are enabled.
        /// </summary>
        public bool EnableDirectives { get; }

        /// <summary>
        /// Gets whether POSIX bundling is enabled.
        /// </summary>
        /// <remarks>
        /// POSIX recommends that single-character options be allowed to be specified together after a single <c>-</c> prefix.
        /// </remarks>
        public bool EnablePosixBundling { get; }

        /// <summary>
        /// Gets the validation messages.
        /// </summary>
        public ValidationMessages ValidationMessages { get; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory => _helpBuilderFactory;

        internal IReadOnlyCollection<InvocationMiddleware> Middleware => _middlewarePipeline;

        /// <summary>
        /// Gets the root command.
        /// </summary>
        public ICommand RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
