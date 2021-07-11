// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Collections;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Represents the configuration used by the <see cref="Parser"/>.
    /// </summary>
    public class CommandLineConfiguration
    {
        private readonly SymbolSet _symbols = new();
        private Func<BindingContext, IHelpBuilder>? _helpBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the CommandLineConfiguration class.
        /// </summary>
        /// <param name="symbols">The symbols to parse.</param>
        /// <param name="enablePosixBundling"><see langword="true"/> to enable POSIX bundling; otherwise, <see langword="false"/>.</param>
        /// <param name="enableDirectives"><see langword="true"/> to enable directive parsing; otherwise, <see langword="false"/>.</param>
        /// <param name="resources">Provide custom validation messages.</param>
        /// <param name="responseFileHandling">One of the enumeration values that specifies how response files (.rsp) are handled.</param>
        /// <param name="middlewarePipeline">Provide a custom middleware pipeline.</param>
        /// <param name="helpBuilderFactory">Provide a custom help builder.</param>
        /// <param name="configureHelp">Configures the help builder.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="symbols"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="symbols"/> does not contain at least one option or command.</exception>
        public CommandLineConfiguration(
            IReadOnlyList<Symbol> symbols,
            bool enablePosixBundling = true,
            bool enableDirectives = true,
            Resources? resources = null,
            ResponseFileHandling responseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            IReadOnlyCollection<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, IHelpBuilder>? helpBuilderFactory = null,
            Action<IHelpBuilder>? configureHelp = null)
        {
            if (symbols is null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            if (symbols.Count == 0)
            {
                throw new ArgumentException("You must specify at least one option or command.");
            }
          
            if (symbols.Count == 1 &&
                symbols[0] is Command rootCommand)
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

                    for (var i = 0; i < symbols.Count; i++)
                    {
                        var symbol = symbols[i];
                        parentRootCommand.Add(symbol);
                    }
                }

                RootCommand = rootCommand = parentRootCommand;
            }

            _symbols.Add(RootCommand);

            AddGlobalOptionsToChildren(rootCommand);

            EnablePosixBundling = enablePosixBundling;
            EnableDirectives = enableDirectives;
            Resources = resources ?? Resources.Instance;
            ResponseFileHandling = responseFileHandling;
            Middleware = middlewarePipeline ?? new List<InvocationMiddleware>();

            _helpBuilderFactory = helpBuilderFactory;

            if (configureHelp != null)
            {
                var factory = HelpBuilderFactory;
                _helpBuilderFactory = context =>
                {
                    IHelpBuilder helpBuilder = factory(context);
                    configureHelp(helpBuilder);
                    return helpBuilder;
                };
            }
        }

        private static IHelpBuilder DefaultHelpBuilderFactory(BindingContext context)
        {
            int maxWidth = int.MaxValue;
            if (context.Console is SystemConsole systemConsole)
            {
                maxWidth = systemConsole.GetWindowWidth();
            }

            return new HelpBuilder(context.Console, context.ParseResult.CommandResult.Resources, maxWidth);
        }

        private void AddGlobalOptionsToChildren(Command parentCommand)
        {
            for (var childIndex = 0; childIndex < parentCommand.Children.Count; childIndex++)
            {
                var child = parentCommand.Children[childIndex];

                if (child is Command childCommand)
                {
                    var globalOptions = parentCommand.GlobalOptions;

                    for (var globalOptionIndex = 0; globalOptionIndex < globalOptions.Count; globalOptionIndex++)
                    {
                        childCommand.TryAddGlobalOption(globalOptions[globalOptionIndex]);
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
        /// Gets the localizable resources.
        /// </summary>
        public Resources Resources { get; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory => _helpBuilderFactory ??= DefaultHelpBuilderFactory;

        internal IReadOnlyCollection<InvocationMiddleware> Middleware { get; }

        /// <summary>
        /// Gets the root command.
        /// </summary>
        public ICommand RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
