// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedOption : Parsed
    {
        public ParsedOption(Option option, string token = null) :
            base(option, token ?? option?.ToString())
        {
        }
    }
}
