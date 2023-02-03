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
    public class CommandLineBuilder 
    {
        /// <inheritdoc cref="CommandLineConfiguration.EnablePosixBundling"/>
        internal bool EnablePosixBundling = true;

        /// <inheritdoc cref="CommandLineConfiguration.EnableTokenReplacement"/>
        internal bool EnableTokenReplacement = true;

        /// <inheritdoc cref="CommandLineConfiguration.ParseErrorReportingExitCode"/>
        internal int? ParseErrorReportingExitCode;

        /// <inheritdoc cref="CommandLineConfiguration.EnableDirectives"/>
        internal bool EnableDirectives = true;

        /// <inheritdoc cref="CommandLineConfiguration.EnableEnvironmentVariableDirective"/>
        internal bool EnableEnvironmentVariableDirective;

        /// <inheritdoc cref="CommandLineConfiguration.ParseDirectiveExitCode"/>
        internal int? ParseDirectiveExitCode;

        /// <inheritdoc cref="CommandLineConfiguration.EnableSuggestDirective"/>
        internal bool EnableSuggestDirective;

        /// <inheritdoc cref="CommandLineConfiguration.MaxLevenshteinDistance"/>
        internal int MaxLevenshteinDistance;

        /// <inheritdoc cref="CommandLineConfiguration.ExceptionHandler"/>
        internal Action<Exception, InvocationContext>? ExceptionHandler;

        // for every generic type with type argument being struct JIT needs to compile a dedicated version
        // (because each struct is of a different size)
        // that is why we don't use List<ValueTuple> for middleware
        private List<Tuple<InvocationMiddleware, int>>? _middlewareList;
        private LocalizationResources? _localizationResources;
        private Action<HelpContext>? _customizeHelpBuilder;
        private Func<BindingContext, HelpBuilder>? _helpBuilderFactory;

        /// <param name="rootCommand">The root command of the application.</param>
        public CommandLineBuilder(Command? rootCommand = null)
        {
            Command = rootCommand ?? new RootCommand();
        }

        /// <summary>
        /// The command that the builder uses the root of the parser.
        /// </summary>
        public Command Command { get; }

        internal void CustomizeHelpLayout(Action<HelpContext> customize) => 
            _customizeHelpBuilder = customize;

        internal void UseHelpBuilderFactory(Func<BindingContext, HelpBuilder> factory) =>
            _helpBuilderFactory = factory;

        private Func<BindingContext, HelpBuilder> GetHelpBuilderFactory()
        {
            return CreateHelpBuilder;

            HelpBuilder CreateHelpBuilder(BindingContext bindingContext)
            {
                var helpBuilder = _helpBuilderFactory is { }
                                             ? _helpBuilderFactory(bindingContext)
                                             : CommandLineConfiguration.DefaultHelpBuilderFactory(bindingContext, MaxHelpWidth);

                helpBuilder.OnCustomize = _customizeHelpBuilder;

                return helpBuilder;
            }
        }

        internal HelpOption? HelpOption;

        internal VersionOption? VersionOption;

        internal int? MaxHelpWidth;

        internal LocalizationResources LocalizationResources
        {
            get => _localizationResources ??= LocalizationResources.Instance;
            set => _localizationResources = value;
        }

        internal TryReplaceToken? TokenReplacer;

        /// <summary>
        /// Creates a parser based on the configuration of the command line builder.
        /// </summary>
        public Parser Build() =>
            new(
                new CommandLineConfiguration(
                    Command,
                    enablePosixBundling: EnablePosixBundling,
                    enableDirectives: EnableDirectives,
                    enableTokenReplacement: EnableTokenReplacement,
                    enableEnvironmentVariableDirective: EnableEnvironmentVariableDirective,
                    parseDirectiveExitCode: ParseDirectiveExitCode,
                    enableSuggestDirective: EnableSuggestDirective,
                    parseErrorReportingExitCode: ParseErrorReportingExitCode,
                    maxLevenshteinDistance: MaxLevenshteinDistance,
                    exceptionHandler: ExceptionHandler,
                    resources: LocalizationResources,
                    middlewarePipeline: _middlewareList is null
                                            ? Array.Empty<InvocationMiddleware>()
                                            : GetMiddleware(),
                    helpBuilderFactory: GetHelpBuilderFactory(),
                    tokenReplacer: TokenReplacer));

        private IReadOnlyList<InvocationMiddleware> GetMiddleware()
        {
            _middlewareList!.Sort(static (m1, m2) => m1.Item2.CompareTo(m2.Item2));
            InvocationMiddleware[] result = new InvocationMiddleware[_middlewareList.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _middlewareList[i].Item1;
            }
            return result;
        }

        internal void AddMiddleware(InvocationMiddleware middleware, MiddlewareOrder order)
            => AddMiddleware(middleware, (int)order);

        internal void AddMiddleware(InvocationMiddleware middleware, MiddlewareOrderInternal order)
            => AddMiddleware(middleware, (int)order);

        private void AddMiddleware(InvocationMiddleware middleware, int order)
            => (_middlewareList ??= new()).Add(new Tuple<InvocationMiddleware, int>(middleware, order));
    }
}
