// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Builder
{
    public class CommandLineBuilder : CommandDefinitionBuilder
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        private List<(InvocationMiddleware middleware, int order)> _middlewareList;

        public CommandLineBuilder() : base(executableName.Value)
        {
        }

        public static string ExeName { get; } = executableName.Value;

        public bool EnablePosixBundling { get; set; } = true;

        public IReadOnlyCollection<string> Prefixes { get; set; }

        public ResponseFileHandling ResponseFileHandling { get; set; }

        public Parser Build()
        {
            var rootCommand = BuildCommandDefinition();

            return new Parser(
                new CommandLineConfiguration(
                    new[] { rootCommand },
                    prefixes: Prefixes,
                    allowUnbundling: EnablePosixBundling,
                    validationMessages: ValidationMessages.Instance,
                    responseFileHandling: ResponseFileHandling,
                    middlewarePipeline: _middlewareList?.OrderBy(m => m.order)
                                                       .Select(m => m.middleware)
                                                       .ToArray()));
        }

        internal void AddMiddleware(
            InvocationMiddleware middleware,
            int order)
        {
            if (_middlewareList == null)
            {
                _middlewareList = new List<(InvocationMiddleware, int)>();
            }

            _middlewareList.Add((middleware, order));
        }

        internal static class MiddlewareOrder
        {
            public const int ExceptionHandler = int.MinValue;
            public const int Preprocessing = ExceptionHandler + 1;
            public const int AfterPreprocessing = Preprocessing + 1;
            public const int Middle = 0;
        }
    }
}
