// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
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
        private Func<BindingContext, HelpBuilder>? _helpBuilderFactory;
        private TryReplaceToken? _tokenReplacer;

        /// <summary>
        /// Initializes a new instance of the CommandLineConfiguration class.
        /// </summary>
        /// <param name="command">The root command for the parser.</param>
        /// <param name="enablePosixBundling"><see langword="true"/> to enable POSIX bundling; otherwise, <see langword="false"/>.</param>
        /// <param name="enableDirectives"><see langword="true"/> to enable directive parsing; otherwise, <see langword="false"/>.</param>
        /// <param name="enableLegacyDoubleDashBehavior">Enables the legacy behavior of the <c>--</c> token, which is to ignore parsing of subsequent tokens and place them in the <see cref="ParseResult.UnparsedTokens"/> list.</param>
        /// <param name="enableTokenReplacement"><see langword="true"/> to enable token replacement; otherwise, <see langword="false"/>.</param>
        /// <param name="resources">Provide custom validation messages.</param>
        /// <param name="middlewarePipeline">Provide a custom middleware pipeline.</param>
        /// <param name="helpBuilderFactory">Provide a custom help builder.</param>
        /// <param name="tokenReplacer">Replaces the specified token with any number of other tokens.</param>
        public CommandLineConfiguration(
            Command command,
            bool enablePosixBundling = true,
            bool enableDirectives = true,
            bool enableLegacyDoubleDashBehavior = false,
            bool enableTokenReplacement = true,
            LocalizationResources? resources = null,
            IReadOnlyList<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, HelpBuilder>? helpBuilderFactory = null,
            TryReplaceToken? tokenReplacer = null)
        {
            RootCommand = command ?? throw new ArgumentNullException(nameof(command));

            EnableLegacyDoubleDashBehavior = enableLegacyDoubleDashBehavior;
            EnableTokenReplacement = enableTokenReplacement;
            EnablePosixBundling = enablePosixBundling;
            EnableDirectives = enableDirectives;

            LocalizationResources = resources ?? LocalizationResources.Instance;
            Middleware = middlewarePipeline ?? Array.Empty<InvocationMiddleware>();

            _helpBuilderFactory = helpBuilderFactory;
            _tokenReplacer = tokenReplacer;
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
        /// Gets whether directives are enabled.
        /// </summary>
        public bool EnableDirectives { get; }

        /// <summary>
        /// Enables the legacy behavior of the <c>--</c> token, which is to ignore parsing of subsequent tokens and place them in the <see cref="ParseResult.UnparsedTokens"/> list.
        /// </summary>
        public bool EnableLegacyDoubleDashBehavior { get; }

        /// <summary>
        /// Gets a value indicating whether POSIX bundling is enabled.
        /// </summary>
        /// <remarks>
        /// POSIX recommends that single-character options be allowed to be specified together after a single <c>-</c> prefix.
        /// </remarks>
        public bool EnablePosixBundling { get; }

        /// <summary>
        /// Gets a value indicating whether token replacement is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, any token prefixed with <code>@</code> can be replaced with zero or more other tokens. This is mostly commonly used to expand tokens from response files and interpolate them into a command line prior to parsing.
        /// </remarks>
        public bool EnableTokenReplacement { get; }

        /// <summary>
        /// Gets the localizable resources.
        /// </summary>
        public LocalizationResources LocalizationResources { get; }

        internal Func<BindingContext, HelpBuilder> HelpBuilderFactory => _helpBuilderFactory ??= context => DefaultHelpBuilderFactory(context);

        internal IReadOnlyList<InvocationMiddleware> Middleware { get; }

        internal TryReplaceToken? TokenReplacer =>
            EnableTokenReplacement
                ? _tokenReplacer ??= DefaultTokenReplacer
                : null;

        private bool DefaultTokenReplacer(
            string tokenToReplace, 
            out IReadOnlyList<string>? replacementTokens, 
            out string? errorMessage) =>
            StringExtensions.TryReadResponseFile(
                tokenToReplace,
                LocalizationResources,
                out replacementTokens,
                out errorMessage);

        /// <summary>
        /// Gets the root command.
        /// </summary>
        public Command RootCommand { get; }

        /// <summary>
        /// Throws an exception if the parser configuration is ambiguous or otherwise not valid.
        /// </summary>
        /// <remarks>Due to the performance cost of this method, it is recommended to be used in unit testing or in scenarios where the parser is configured dynamically at runtime.</remarks>
        /// <exception cref="CommandLineConfigurationException">Thrown if the configuration is found to be invalid.</exception>
        public void ThrowIfInvalid()
        {
            ThrowIfInvalid(RootCommand);

            static void ThrowIfInvalid(Command command)
            {
                if (command.Parents.FlattenBreadthFirst(c => c.Parents).Any(ancestor => ancestor == command))
                {
                    throw new CommandLineConfigurationException($"Cycle detected in command tree. Command '{command.Name}' is its own ancestor.");
                }

                int count = command.Subcommands.Count + command.Options.Count;
                for (var i = 0; i < count; i++)
                {
                    IdentifierSymbol symbol1AsIdentifier = GetChild(i, command);
                    for (var j = i + 1; j < count; j++)
                    {
                        IdentifierSymbol symbol2AsIdentifier = GetChild(j, command);

                        foreach (var symbol2Alias in symbol2AsIdentifier.Aliases)
                        {
                            if (symbol1AsIdentifier.Name.Equals(symbol2Alias, StringComparison.Ordinal) ||
                                symbol1AsIdentifier.Aliases.Contains(symbol2Alias))
                            {
                                throw new CommandLineConfigurationException($"Duplicate alias '{symbol2Alias}' found on command '{command.Name}'.");
                            }
                        }
                    }

                    if (symbol1AsIdentifier is Command childCommand)
                    {
                        ThrowIfInvalid(childCommand);
                    }
                }
            }

            static IdentifierSymbol GetChild(int index, Command command)
                => index < command.Subcommands.Count
                    ? command.Subcommands[index]
                    : command.Options[index - command.Subcommands.Count];
        }
    }
}