// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentsRuleHelp
    {
        public ArgumentsRuleHelp()
        {
        }

        public ArgumentsRuleHelp(string name, string description)
        {
            Description = description;
            Name = name;
        }

        public string Description { get; }

        public string Name { get; }
    }
}