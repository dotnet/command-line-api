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
        /// <param name="symbol">The symbol to parse.</param>
        /// <param name="enablePosixBundling"><see langword="true"/> to enable POSIX bundling; otherwise, <see langword="false"/>.</param>
        /// <param name="enableDirectives"><see langword="true"/> to enable directive parsing; otherwise, <see langword="false"/>.</param>
        /// <param name="enableLegacyDoubleDashBehavior">Enables the legacy behavior of the <c>--</c> token, which is to ignore parsing of subsequent tokens and place them in the <see cref="ParseResult.UnparsedTokens"/> list.</param>
        /// <param name="resources">Provide custom validation messages.</param>
        /// <param name="responseFileHandling">One of the enumeration values that specifies how response files (.rsp) are handled.</param>
        /// <param name="middlewarePipeline">Provide a custom middleware pipeline.</param>
        /// <param name="helpBuilderFactory">Provide a custom help builder.</param>
        /// <param name="configureHelp">Configures the help builder.</param>
        public CommandLineConfiguration(
            Symbol symbol,
            bool enablePosixBundling = true,
            bool enableDirectives = true,
            bool enableLegacyDoubleDashBehavior = false,
            LocalizationResources? resources = null,
            ResponseFileHandling responseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            IReadOnlyCollection<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, IHelpBuilder>? helpBuilderFactory = null,
            Action<IHelpBuilder>? configureHelp = null)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (symbol is Command rootCommand)
            {
                RootCommand = rootCommand;
            }
            else
            {
                rootCommand = new RootCommand();

                rootCommand.Add(symbol);

                RootCommand = rootCommand;
            }

            _symbols.Add(RootCommand);

            AddGlobalOptionsToChildren(rootCommand);

            EnableLegacyDoubleDashBehavior = enableLegacyDoubleDashBehavior;
            EnablePosixBundling = enablePosixBundling;
            EnableDirectives = enableDirectives;
            LocalizationResources = resources ?? LocalizationResources.Instance;
            ResponseFileHandling = responseFileHandling;
            Middleware = middlewarePipeline ?? Array.Empty<InvocationMiddleware>();

            _helpBuilderFactory = helpBuilderFactory;

            if (configureHelp is { })
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

            return new HelpBuilder(context.ParseResult.CommandResult.LocalizationResources, maxWidth);
        }

        private void AddGlobalOptionsToChildren(Command parentCommand)
        {
            for (var childIndex = 0; childIndex < parentCommand.Children.Count; childIndex++)
            {
                var child = parentCommand.Children[childIndex];

                if (child is Command childCommand)
                {
                    var globalOptions = parentCommand.GlobalOptions;

                    for (var i = 0; i < globalOptions.Count; i++)
                    {
                        childCommand.TryAddGlobalOption(globalOptions[i]);
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
        /// Enables the legacy behavior of the <c>--</c> token, which is to ignore parsing of subsequent tokens and place them in the <see cref="ParseResult.UnparsedTokens"/> list.
        /// </summary>
        public bool EnableLegacyDoubleDashBehavior { get; }

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
        public LocalizationResources LocalizationResources { get; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory => _helpBuilderFactory ??= DefaultHelpBuilderFactory;

        internal IReadOnlyCollection<InvocationMiddleware> Middleware { get; }

        /// <summary>
        /// Gets the root command.
        /// </summary>
        public ICommand RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }
    }
}
