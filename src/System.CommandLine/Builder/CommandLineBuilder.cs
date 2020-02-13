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
        private readonly List<(InvocationMiddleware middleware, int order)> _middlewareList = new List<(InvocationMiddleware middleware, int order)>();

        public CommandLineBuilder(Command rootCommand = null)
            : base(rootCommand ?? new RootCommand())
        {
            if (rootCommand?.ImplicitParser != null)
            {
                throw new ArgumentException($"Command \"{rootCommand.Name}\" has already been configured.");
            }
        }

        public bool EnableDirectives { get; set; } = true;

        public bool EnablePosixBundling { get; set; } = true;

        public ResponseFileHandling ResponseFileHandling { get; set; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory { get; set; }

        internal Option HelpOption { get; set; }

        public Parser Build()
        {
            var rootCommand = Command;

            var parser = new Parser(
                new CommandLineConfiguration(
                    new[] { rootCommand },
                    enablePosixBundling: EnablePosixBundling,
                    enableDirectives: EnableDirectives,
                    validationMessages: ValidationMessages.Instance,
                    responseFileHandling: ResponseFileHandling,
                    middlewarePipeline: _middlewareList?.OrderBy(m => m.order)
                                                       .Select(m => m.middleware)
                                                       .ToArray(),
                    helpBuilderFactory: HelpBuilderFactory));

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
