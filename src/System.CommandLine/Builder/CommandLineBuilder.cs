// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Enables composition of command line configurations.
    /// </summary>
    public partial class CommandLineBuilder 
    {
        /// <inheritdoc cref="CommandLineConfiguration.EnablePosixBundling"/>
        internal bool EnablePosixBundlingFlag = true;

        /// <inheritdoc cref="CommandLineConfiguration.EnableTokenReplacement"/>
        internal bool EnableTokenReplacement = true;

        /// <inheritdoc cref="CommandLineConfiguration.ParseErrorReportingExitCode"/>
        internal int? ParseErrorReportingExitCode;

        /// <inheritdoc cref="CommandLineConfiguration.MaxLevenshteinDistance"/>
        internal int MaxLevenshteinDistance;

        /// <inheritdoc cref="CommandLineConfiguration.ExceptionHandler"/>
        internal Func<Exception, InvocationContext, int>? ExceptionHandler;

        internal TimeSpan? ProcessTerminationTimeout;


        /// <param name="rootCommand">The root command of the application.</param>
        public CommandLineBuilder(Command rootCommand)
        {
            Command = rootCommand ?? throw new ArgumentNullException(nameof(rootCommand));
        }

        /// <summary>
        /// The command that the builder uses the root of the parser.
        /// </summary>
        public Command Command { get; }

        internal HelpOption? HelpOption;

        internal VersionOption? VersionOption;

        internal TryReplaceToken? TokenReplacer;

        public List<Directive> Directives => _directives ??= new ();

        private List<Directive>? _directives;

        /// <summary>
        /// Creates a parser based on the configuration of the command line builder.
        /// </summary>
        public CommandLineConfiguration Build() =>
            new (
                Command,
                _directives,
                enablePosixBundling: EnablePosixBundlingFlag,
                enableTokenReplacement: EnableTokenReplacement,
                parseErrorReportingExitCode: ParseErrorReportingExitCode,
                maxLevenshteinDistance: MaxLevenshteinDistance,
                exceptionHandler: ExceptionHandler,
                processTerminationTimeout: ProcessTerminationTimeout,
                tokenReplacer: TokenReplacer);
    }
}
