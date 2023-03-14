// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
        internal readonly Func<Exception, InvocationContext, int>? ExceptionHandler;

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
        /// <param name="enableTokenReplacement"><see langword="true"/> to enable token replacement; otherwise, <see langword="false"/>.</param>
        /// <param name="middlewarePipeline">Provide a custom middleware pipeline.</param>
        /// <param name="helpBuilderFactory">Provide a custom help builder.</param>
        /// <param name="tokenReplacer">Replaces the specified token with any number of other tokens.</param>
        public CommandLineConfiguration(
            Command command,
            bool enablePosixBundling = true,
            bool enableTokenReplacement = true,
            IReadOnlyList<InvocationMiddleware>? middlewarePipeline = null,
            Func<BindingContext, HelpBuilder>? helpBuilderFactory = null,
            TryReplaceToken? tokenReplacer = null)
            : this(
                  command,
                  directives: null,
                  enablePosixBundling: enablePosixBundling,
                  enableTokenReplacement: enableTokenReplacement,
                  parseErrorReportingExitCode: null,
                  maxLevenshteinDistance: 0,
                  processTerminationTimeout: null,
                  middlewarePipeline: middlewarePipeline,
                  helpBuilderFactory: helpBuilderFactory,
                  tokenReplacer: tokenReplacer,
                  exceptionHandler: null)
        {
        }

        internal CommandLineConfiguration(
            Command command,
            List<Directive>? directives,
            bool enablePosixBundling,
            bool enableTokenReplacement,
            int? parseErrorReportingExitCode,
            int maxLevenshteinDistance,
            TimeSpan? processTerminationTimeout,
            IReadOnlyList<InvocationMiddleware>? middlewarePipeline,
            Func<BindingContext, HelpBuilder>? helpBuilderFactory,
            TryReplaceToken? tokenReplacer,
            Func<Exception, InvocationContext, int>? exceptionHandler)
        {
            RootCommand = command ?? throw new ArgumentNullException(nameof(command));
            Directives = directives is not null ? directives : Array.Empty<Directive>();
            EnableTokenReplacement = enableTokenReplacement;
            EnablePosixBundling = enablePosixBundling;
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
        /// Gets the enabled directives.
        /// </summary>
        public IReadOnlyList<Directive> Directives { get; }

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
                    Symbol symbol1 = GetChild(i, command, out AliasSet? aliases1);
                    for (var j = i + 1; j < count; j++)
                    {
                        Symbol symbol2 = GetChild(j, command, out AliasSet? aliases2);

                        if (symbol1.Name.Equals(symbol2.Name, StringComparison.Ordinal)
                            || (aliases1 is not null && aliases1.Contains(symbol2.Name)))
                        {
                            throw new CommandLineConfigurationException($"Duplicate alias '{symbol2.Name}' found on command '{command.Name}'.");
                        }
                        else if (aliases2 is not null && aliases2.Contains(symbol1.Name))
                        {
                            throw new CommandLineConfigurationException($"Duplicate alias '{symbol1.Name}' found on command '{command.Name}'.");
                        }

                        if (aliases1 is not null && aliases2 is not null)
                        {
                            // take advantage of the fact that we are dealing with two hash sets
                            if (aliases1.Overlaps(aliases2))
                            {
                                foreach (string symbol2Alias in aliases2)
                                {
                                    if (aliases1.Contains(symbol2Alias))
                                    {
                                        throw new CommandLineConfigurationException($"Duplicate alias '{symbol2Alias}' found on command '{command.Name}'.");
                                    }
                                }
                            }
                        }
                    }

                    if (symbol1 is Command childCommand)
                    {
                        ThrowIfInvalid(childCommand);
                    }
                }
            }

            static Symbol GetChild(int index, Command command, out AliasSet? aliases)
            {
                if (index < command.Subcommands.Count)
                {
                    aliases = command.Subcommands[index]._aliases;
                    return command.Subcommands[index];
                }

                aliases = command.Options[index - command.Subcommands.Count]._aliases;
                return command.Options[index - command.Subcommands.Count];
            }
        }
    }
}