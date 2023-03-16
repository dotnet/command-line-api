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

        private Action<HelpContext>? _customizeHelpBuilder;
        private Func<InvocationContext, HelpBuilder>? _helpBuilderFactory;

        /// <param name="rootCommand">The root command of the application.</param>
        public CommandLineBuilder(Command rootCommand)
        {
            Command = rootCommand ?? throw new ArgumentNullException(nameof(rootCommand));
        }

        /// <summary>
        /// The command that the builder uses the root of the parser.
        /// </summary>
        public Command Command { get; }

        internal void CustomizeHelpLayout(Action<HelpContext> customize) => 
            _customizeHelpBuilder = customize;

        internal void UseHelpBuilderFactory(Func<InvocationContext, HelpBuilder> factory) =>
            _helpBuilderFactory = factory;

        private Func<InvocationContext, HelpBuilder> GetHelpBuilderFactory()
        {
            return CreateHelpBuilder;

            HelpBuilder CreateHelpBuilder(InvocationContext invocationContext)
            {
                var helpBuilder = _helpBuilderFactory is { }
                                             ? _helpBuilderFactory(invocationContext)
                                             : CommandLineConfiguration.DefaultHelpBuilderFactory(invocationContext, MaxHelpWidth);

                helpBuilder.OnCustomize = _customizeHelpBuilder;

                return helpBuilder;
            }
        }

        internal HelpOption? HelpOption;

        internal VersionOption? VersionOption;

        internal int? MaxHelpWidth;

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
                helpBuilderFactory: GetHelpBuilderFactory(),
                tokenReplacer: TokenReplacer);
    }
}
