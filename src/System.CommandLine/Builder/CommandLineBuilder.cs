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
    public class CommandLineBuilder 
    {
        private readonly List<(InvocationMiddleware middleware, int order)> _middlewareList = new();
        private LocalizationResources? _localizationResources;

        /// <param name="rootCommand">The root command of the application.</param>
        public CommandLineBuilder(Command? rootCommand = null)
        {
            Command = rootCommand ?? new RootCommand();
        }

        /// <summary>
        /// The command that the builder uses the root of the parser.
        /// </summary>
        public Command Command { get; }

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
        /// Determines the behavior when parsing a double dash (<c>--</c>) in a command line.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, all tokens following <c>--</c> will be placed into the <see cref="ParseResult.UnparsedTokens"/> collection. When set to <see langword="false"/>, all tokens following <c>--</c> will be treated as command arguments, even if they match an existing option.</remarks>
        public bool EnableLegacyDoubleDashBehavior { get; set; }

        /// <summary>
        /// Configures the parser's handling of response files. When enabled, a command line token beginning with <c>@</c> that is a valid file path will be expanded as though inserted into the command line. 
        /// </summary>
        public ResponseFileHandling ResponseFileHandling { get; set; }

        internal Func<BindingContext, HelpBuilder>? HelpBuilderFactory { get; set; }

        internal HelpOption? HelpOption { get; set; }

        internal VersionOption? VersionOption { get; set; }

        internal LocalizationResources LocalizationResources
        {
            get => _localizationResources ??= LocalizationResources.Instance;
            set => _localizationResources = value;
        }
        
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
                    enableLegacyDoubleDashBehavior: EnableLegacyDoubleDashBehavior,
                    resources: LocalizationResources,
                    responseFileHandling: ResponseFileHandling,
                    middlewarePipeline: _middlewareList.OrderBy(m => m.order)
                                                       .Select(m => m.middleware)
                                                       .ToArray(),
                    helpBuilderFactory: HelpBuilderFactory));
            
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
