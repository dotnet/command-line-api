// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Collections;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Represents the configuration used by the <see cref="Parser"/>.
    /// </summary>
    public class CommandLineConfiguration
    {
        private Func<BindingContext, HelpBuilder>? _helpBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the CommandLineConfiguration class.
        /// </summary>
        /// <param name="command">The root command for the parser.</param>
        /// <param name="enablePosixBundling"><see langword="true"/> to enable POSIX bundling; otherwise, <see langword="false"/>.</param>
        /// <param name="enableDirectives"><see langword="true"/> to enable directive parsing; otherwise, <see langword="false"/>.</param>
        /// <param name="enableLegacyDoubleDashBehavior">Enables the legacy behavior of the <c>--</c> token, which is to ignore parsing of subsequent tokens and place them in the <see cref="ParseResult.UnparsedTokens"/> list.</param>
        /// <param name="resources">Provide custom validation messages.</param>
        /// <param name="responseFileHandling">One of the enumeration values that specifies how response files (.rsp) are handled.</param>
        /// <param name="middlewarePipeline">Provide a custom middleware pipeline.</param>
        /// <param name="helpBuilderFactory">Provide a custom help builder.</param>
        public CommandLineConfiguration(
            Command command,
            bool enablePosixBundling = true,
            bool enableDirectives = true,
            bool enableLegacyDoubleDashBehavior = false,
            LocalizationResources? resources = null,
            ResponseFileHandling responseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
            IReadOnlyList<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, HelpBuilder>? helpBuilderFactory = null)
        {
            RootCommand = command ?? throw new ArgumentNullException(nameof(command));

            EnableLegacyDoubleDashBehavior = enableLegacyDoubleDashBehavior;
            EnablePosixBundling = enablePosixBundling;
            EnableDirectives = enableDirectives;
            LocalizationResources = resources ?? LocalizationResources.Instance;
            ResponseFileHandling = responseFileHandling;
            Middleware = middlewarePipeline ?? Array.Empty<InvocationMiddleware>();

            _helpBuilderFactory = helpBuilderFactory;
        }

        internal static HelpBuilder DefaultHelpBuilderFactory(BindingContext context, int? requestedMaxWidth = null)
        {
            int maxWidth = requestedMaxWidth ?? int.MaxValue;           
            if (context.Console is SystemConsole systemConsole)
            {
                maxWidth = systemConsole.GetWindowWidth();
            }

            return new HelpBuilder(context.ParseResult.CommandResult.LocalizationResources, maxWidth);
        }

        /// <summary>
        /// Represents all of the symbols to parse.
        /// </summary>
        public SymbolSet Symbols => RootCommand.Children;

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

        internal Func<BindingContext, HelpBuilder> HelpBuilderFactory => _helpBuilderFactory ??= (context) => DefaultHelpBuilderFactory(context);

        internal IReadOnlyList<InvocationMiddleware> Middleware { get; }

        /// <summary>
        /// Gets the root command.
        /// </summary>
        public ICommand RootCommand { get; }

        internal ResponseFileHandling ResponseFileHandling { get; }

        /// <summary>
        /// Validates all symbols including the child hierarchy.
        /// </summary>
        /// <remarks>Due to the performance impact of this method, it's recommended to create
        /// a Unit Test that calls this method to verify the RootCommand of every application.</remarks>
        internal void ThrowIfInvalid()
        {
            ThrowIfInvalid((Command)RootCommand);

            static void ThrowIfInvalid(Command command)
            {
                for (int i = 0; i < command.Children.Count; i++)
                {
                    for (int j = 1; j < command.Children.Count; j++)
                    {
                        if (command.Children[j] is IdentifierSymbol identifierSymbol)
                        {
                            foreach (string alias in identifierSymbol.Aliases)
                            {
                                if (command.Children[i].Matches(alias))
                                {
                                    throw new ArgumentException($"Alias '{alias}' is already in use.");
                                }
                            }

                            if (identifierSymbol is Command childCommand)
                            {
                                if (ReferenceEquals(command, childCommand))
                                {
                                    throw new ArgumentException("Parent can't be it's own child.");
                                }

                                ThrowIfInvalid(childCommand);
                            }
                        }

                        if (command.Children[i].Matches(command.Children[j].Name))
                        {
                            throw new ArgumentException($"Alias '{command.Children[j].Name}' is already in use.");
                        }
                    }

                    if (command.Children.Count == 1 && command.Children[0] is Command singleChild)
                    {
                        if (ReferenceEquals(command, singleChild))
                        {
                            throw new ArgumentException("Parent can't be it's own child.");
                        }

                        ThrowIfInvalid(singleChild);
                    }
                }
            }
        }
    }
}
