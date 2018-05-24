// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Builder
{
    public class ParserBuilder : CommandDefinitionBuilder
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        private List<InvocationMiddleware> _invocationList;

        public ParserBuilder() : base(executableName.Value)
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
                new ParserConfiguration(
                    new[] { rootCommand },
                    prefixes: Prefixes,
                    allowUnbundling: EnablePosixBundling,
                    validationMessages: ValidationMessages.Instance,
                    responseFileHandling: ResponseFileHandling,
                    invocationList: _invocationList));
        }

        internal void AddInvocation(InvocationMiddleware action)
        {
            if (_invocationList == null)
            {
                _invocationList = new List<InvocationMiddleware>();
            }

            _invocationList.Add(action);
        }
    }
}
