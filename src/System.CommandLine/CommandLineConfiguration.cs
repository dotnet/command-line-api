﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace System.CommandLine
{
    /// <summary>
    /// Represents the configuration used by the <see cref="Parser"/>.
    /// </summary>
    public class CommandLineConfiguration
    {
        /// <summary>
        /// A delegate that will be called when an exception is thrown by a command handler.
        /// </summary>
        internal readonly Action<Exception, InvocationContext>? ExceptionHandler;

        /// <summary>
        /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
        /// </summary>
        internal readonly bool EnableEnvironmentVariableDirective;

        /// <summary>
        /// If the parse result contains errors, this exit code will be used when the process exits.
        /// </summary>
        internal readonly int? ParseDirectiveExitCode;

        /// <summary>
        /// Enables the use of the <c>[suggest]</c> directive which when specified in command line input short circuits normal command handling and writes a newline-delimited list of suggestions suitable for use by most shells to provide command line completions.
        /// </summary>
        /// <remarks>The <c>dotnet-suggest</c> tool requires the suggest directive to be enabled for an application to provide completions.</remarks>
        internal readonly bool EnableSuggestDirective;

        /// <summary>
        /// The exit code to use when parser errors occur.
        /// </summary>
        internal readonly int? ParseErrorReportingExitCode;

        /// <summary>
        /// The maximum Levenshtein distance for suggestions based on detected typos in command line input.
        /// </summary>
        internal readonly int MaxLevenshteinDistance;

        internal readonly TimeSpan? ProcessTerminationTimeout;

        internal readonly IReadOnlyList<InvocationMiddleware> Middleware;

        private Func<BindingContext, HelpBuilder>? _helpBuilderFactory;
        private TryReplaceToken? _tokenReplacer;

        /// <summary>
        /// Initializes a new instance of the CommandLineConfiguration class.
        /// </summary>
        /// <param name="command">The root command for the parser.</param>
        /// <param name="enablePosixBundling"><see langword="true"/> to enable POSIX bundling; otherwise, <see langword="false"/>.</param>
        /// <param name="enableDirectives"><see langword="true"/> to enable directive parsing; otherwise, <see langword="false"/>.</param>
        /// <param name="enableTokenReplacement"><see langword="true"/> to enable token replacement; otherwise, <see langword="false"/>.</param>
        /// <param name="middlewarePipeline">Provide a custom middleware pipeline.</param>
        /// <param name="helpBuilderFactory">Provide a custom help builder.</param>
        /// <param name="tokenReplacer">Replaces the specified token with any number of other tokens.</param>
        public CommandLineConfiguration(
            Command command,
            bool enablePosixBundling = true,
            bool enableDirectives = true,
            bool enableTokenReplacement = true,
            IReadOnlyList<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, HelpBuilder>? helpBuilderFactory = null,
            TryReplaceToken? tokenReplacer = null)
            : this(command, enablePosixBundling, enableDirectives, enableTokenReplacement, false, null, false, null, 0, null,
                  middlewarePipeline, helpBuilderFactory, tokenReplacer, null)
        {
        }

        internal CommandLineConfiguration(
            Command command,
            bool enablePosixBundling,
            bool enableDirectives,
            bool enableTokenReplacement,
            bool enableEnvironmentVariableDirective,
            int? parseDirectiveExitCode,
            bool enableSuggestDirective,
            int? parseErrorReportingExitCode,
            int maxLevenshteinDistance,
            TimeSpan? processTerminationTimeout,
            IReadOnlyList<InvocationMiddleware>? middlewarePipeline,
            Func<BindingContext, HelpBuilder>? helpBuilderFactory,
            TryReplaceToken? tokenReplacer,
            Action<Exception, InvocationContext>? exceptionHandler)
        {
            RootCommand = command ?? throw new ArgumentNullException(nameof(command));
            EnableTokenReplacement = enableTokenReplacement;
            EnablePosixBundling = enablePosixBundling;
            EnableDirectives = enableDirectives || enableEnvironmentVariableDirective || parseDirectiveExitCode.HasValue || enableSuggestDirective;
            EnableEnvironmentVariableDirective = enableEnvironmentVariableDirective;
            ParseDirectiveExitCode = parseDirectiveExitCode;
            EnableSuggestDirective = enableSuggestDirective;
            ParseErrorReportingExitCode = parseErrorReportingExitCode;
            MaxLevenshteinDistance = maxLevenshteinDistance;
            ProcessTerminationTimeout = processTerminationTimeout;
            Middleware = middlewarePipeline ?? Array.Empty<InvocationMiddleware>();

            _helpBuilderFactory = helpBuilderFactory;
            _tokenReplacer = tokenReplacer;
            ExceptionHandler = exceptionHandler;
        }

        public static CommandLineBuilder CreateBuilder(Command rootCommand) => new CommandLineBuilder(rootCommand);

        internal static HelpBuilder DefaultHelpBuilderFactory(BindingContext context, int? requestedMaxWidth = null)
        {
            int maxWidth = requestedMaxWidth ?? int.MaxValue;
            if (requestedMaxWidth is null && context.Console is SystemConsole systemConsole)
            {
                maxWidth = systemConsole.GetWindowWidth();
            }

            return new HelpBuilder(maxWidth);
        }

        /// <summary>
        /// Gets whether directives are enabled.
        /// </summary>
        public bool EnableDirectives { get; }

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

        internal Func<BindingContext, HelpBuilder> HelpBuilderFactory => _helpBuilderFactory ??= context => DefaultHelpBuilderFactory(context);

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
                out replacementTokens,
                out errorMessage);

        /// <summary>
        /// Gets the root command.
        /// </summary>
        public Command RootCommand { get; }

        /// <summary>
        /// Parses a command line string value and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        public int Invoke(string commandLine, IConsole? console = null)
            => RootCommand.Parse(commandLine, this).Invoke(console);

        /// <summary>
        /// Parses a command line string array and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        public int Invoke(string[] args, IConsole? console = null)
            => RootCommand.Parse(args, this).Invoke(console);

        /// <summary>
        /// Parses a command line string value and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        public Task<int> InvokeAsync(string commandLine, IConsole? console = null, CancellationToken cancellationToken = default)
            => RootCommand.Parse(commandLine, this).InvokeAsync(console, cancellationToken);

        /// <summary>
        /// Parses a command line string array and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        public Task<int> InvokeAsync(string[] args, IConsole? console = null, CancellationToken cancellationToken = default)
            => RootCommand.Parse(args, this).InvokeAsync(console, cancellationToken);

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