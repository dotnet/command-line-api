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
        private List<(InvocationMiddleware middleware, MiddlewareOrder order)> _middlewareList;

        public CommandLineBuilder(Command rootCommand = null)
            : base(rootCommand ?? new RootCommand())
        {
        }

        public bool EnableDirectives { get; set; } = true;

        public bool EnablePosixBundling { get; set; } = true;

        public IReadOnlyCollection<string> Prefixes { get; set; }

        public ResponseFileHandling ResponseFileHandling { get; set; }

        internal Func<BindingContext, IHelpBuilder> HelpBuilderFactory { get; set; }

        public Parser Build()
        {
            var rootCommand = Command;

            return new Parser(
                new CommandLineConfiguration(
                    new[] { rootCommand },
                    prefixes: Prefixes,
                    enablePosixBundling: EnablePosixBundling,
                    enableDirectives: EnableDirectives,
                    validationMessages: ValidationMessages.Instance,
                    responseFileHandling: ResponseFileHandling,
                    middlewarePipeline: _middlewareList?.OrderBy(m => m.order)
                                                       .Select(m => m.middleware)
                                                       .ToArray(), 
                    helpBuilderFactory: HelpBuilderFactory));
        }

        internal void AddMiddleware(
            InvocationMiddleware middleware,
            MiddlewareOrder order)
        {
            if (_middlewareList == null)
            {
                _middlewareList = new List<(InvocationMiddleware, MiddlewareOrder)>();
            }

            _middlewareList.Add((middleware, order));
        }
    }
}
