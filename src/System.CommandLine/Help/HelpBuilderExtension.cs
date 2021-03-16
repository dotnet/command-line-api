// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    public static class HelpBuilderExtension
    {
        public static void Customize(this HelpBuilder helpBuilder, 
            IOption option, 
            string? name = null, 
            string[]? aliases = null,
            string? defaultValue = null)
        {
            helpBuilder.Customize(option, () => name, () => aliases, () => defaultValue);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            ICommand command,
            string? name = null)
        {
            helpBuilder.Customize(command, () => name);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            IArgument argument,
            string? name = null,
            string? defaultValue = null)
        {
            helpBuilder.Customize(argument, () => name, () => defaultValue);
        }
    }
}
