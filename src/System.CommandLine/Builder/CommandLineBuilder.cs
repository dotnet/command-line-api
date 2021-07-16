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
    public class CommandLineBuilder : CommandBuilder
    {
        private readonly List<(InvocationMiddleware middleware, int order)> _middlewareList = new();

        public CommandLineBuilder(Command? rootCommand = null)
            : base(rootCommand ?? new RootCommand())
        {
        }

        public bool EnableDirectives { get; set; } = true;

        public bool EnablePosixBundling { get; set; } = true;

        public ResponseFileHandling ResponseFileHandling { get; set; }

        internal Func<BindingContext, IHelpBuilder>? HelpBuilderFactory { get; set; }
        internal Action<IHelpBuilder>? ConfigureHelp { get; set; }

        internal HelpOption? HelpOption { get; set; }
        internal Option<bool>? VersionOption { get; set; }

        internal Resources? Resources { get; set; }

        public Parser Build()
        {
            var resources = Resources ?? Resources.Instance;

            if (HelpOption is not null)
            {
                HelpOption.Description = resources.HelpOptionDescription();
            }

            if (VersionOption is not null)
            {
                VersionOption.Description = resources.VersionOptionDescription();
            }

            var rootCommand = Command;

            var parser = new Parser(
                new CommandLineConfiguration(
                    new[] { rootCommand },
                    enablePosixBundling: EnablePosixBundling,
                    enableDirectives: EnableDirectives,
                    resources: Resources,
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
