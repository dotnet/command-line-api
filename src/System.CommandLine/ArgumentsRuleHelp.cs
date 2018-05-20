// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ArgumentsRuleHelp
    {
        internal const bool DefaultIsHidden = false;

        public ArgumentsRuleHelp()
        {
        }

        public ArgumentsRuleHelp(string name, string description, bool isHidden)
        {
            Description = description;
            IsHidden = isHidden;
            Name = name;
        }

        public string Description { get; }

        public bool IsHidden { get; }

        public string Name { get; }
    }
}
