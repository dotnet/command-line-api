// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class OptionParser : Parser
    {
        public OptionParser(params Option[] options) : base(options)
        {
        }

        public OptionParser(ParserConfiguration configuration) : base(configuration)
        {
        }
    }

    public class CommandParser : Parser
    {
        public CommandParser(params Command[] commands) : base(commands)
        {
        }
    }
}
