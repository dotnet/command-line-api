// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Builder
{
    public class ParserBuilder : CommandDefinitionBuilder
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        private List<InvocationDelegate> _invocationList;

        public ParserBuilder() : base(executableName.Value)
        {
        }

        public static string ExeName { get; } = executableName.Value;

        public bool EnablePosixBundling { get; set; } = true;

        public IReadOnlyCollection<string> Prefixes { get; set; }

        public ResponseFileHandling ResponseFileHandling { get; set; }

        public Parser Build()
        {
            return new Parser(
                new ParserConfiguration(
                    BuildChildSymbolDefinitions(),
                    prefixes: Prefixes,
                    allowUnbundling: EnablePosixBundling,
                    validationMessages: ValidationMessages.Instance,
                    responseFileHandling: ResponseFileHandling,
                    invocationList: _invocationList));
        }

        internal void AddInvocation(InvocationDelegate action)
        {
            if (_invocationList == null)
            {
                _invocationList = new List<InvocationDelegate>();
            }

            _invocationList.Add(action);
        }
    }
}
