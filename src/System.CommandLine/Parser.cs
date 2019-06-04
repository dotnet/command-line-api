// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class Parser
    {
        public Parser(CommandLineConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Parser(params Symbol[] symbols) : this(new CommandLineConfiguration(symbols))
        {
        }

        public Parser() : this(new RootCommand())
        {
        }

        public CommandLineConfiguration Configuration { get; }

        public virtual ParseResult Parse(
            IReadOnlyList<string> arguments,
            string rawInput = null)
        {
            return new CommandLineParser(Configuration).Parse(arguments, rawInput);
        }
    }
}
