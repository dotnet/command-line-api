// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public static class ArgumentsRuleExtensions
    {
        public static ArgumentsRule Named(
            this ArgumentsRule rule,
            string name)
        {
            return rule;
        }

        public static ArgumentsRule Default(
            this ArgumentsRule rule,
            string name)
        {
            return rule;
        }

        public static ArgumentsRule Description(
            this ArgumentsRule rule,
            string description)
        {
            return rule;
        }
    }
}