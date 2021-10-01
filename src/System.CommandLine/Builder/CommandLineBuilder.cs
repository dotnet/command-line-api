// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Builder
{
    /// <summary>
    /// Enables composition of command line configurations.
    /// </summary>
    public class CommandLineBuilder : CommandBuilder
    {
        private readonly List<(InvocationMiddleware middleware, int order)> _middlewareList = new();

        /// <param name="rootCommand">The root command of the application.</param>
        public CommandLineBuilder(Command? rootCommand = null)
            : base(rootCommand ?? new RootCommand())
        {
        }

        /// <summary>
        /// Determines whether the parser recognizes command line directives.
        /// </summary>
        /// <seealso cref="IDirectiveCollection"/>
        public bool EnableDirectives { get; set; } = true;

        /// <summary>
        /// Determines whether the parser recognize and expands POSIX-style bundled options.
        /// </summary>
        public bool EnablePosixBundling { get; set; } = true;

        /// <summary>
        /// Configures the parser's handling of response files. When enabled, a command line token beginning with <c>@</c> that is a valid file path will be expanded as though inserted into the command line. 
        /// </summary>
        public ResponseFileHandling ResponseFileHandling { get; set; }

        internal Func<BindingContext, IHelpBuilder>? HelpBuilderFactory { get; set; }

        internal Action<IHelpBuilder>? ConfigureHelp { get; set; }

        internal HelpOption? HelpOption { get; set; }

        internal VersionOption? VersionOption { get; set; }

        internal LocalizationResources? LocalizationResources { get; set; }

        /// <summary>
        /// Creates a parser based on the configuration of the command line builder.
        /// </summary>
        public Parser Build()
        {
            var parser = new Parser(
                new CommandLineConfiguration(
                    Command,
                    enablePosixBundling: EnablePosixBundling,
                    enableDirectives: EnableDirectives,
                    resources: LocalizationResources,
                    responseFileHandling: ResponseFileHandling,
                    middlewarePipeline: _middlewareList.OrderBy(m => m.order)
                                                       .Select(m => m.middleware)
                                                       .ToArray(),
                    helpBuilderFactory: HelpBuilderFactory,
                    configureHelp: ConfigureHelp));

            Command.ImplicitParser = parser;

            return parser;
        }

        internal void AddMiddleware(
            InvocationMiddleware middleware,
            MiddlewareOrder order)
        {
            _middlewareList.Add((middleware, (int) order));
        }

        internal void AddMiddleware(
            InvocationMiddleware middleware,
            MiddlewareOrderInternal order)
        {
            _middlewareList.Add((middleware, (int) order));
        }
    }
}
