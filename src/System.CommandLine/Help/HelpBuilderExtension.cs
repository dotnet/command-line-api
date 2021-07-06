// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides extension methods for the help builder.
    /// </summary>
    public static class HelpBuilderExtension
    {
        public static void Customize(this HelpBuilder helpBuilder, 
            IOption option, 
            string? descriptor = null, 
            string? defaultValue = null)
        {
            helpBuilder.Customize(option, () => descriptor, () => defaultValue);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            ICommand command,
            string? descriptor = null)
        {
            helpBuilder.Customize(command, () => descriptor);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            IArgument argument,
            string? descriptor = null,
            string? defaultValue = null)
        {
            helpBuilder.Customize(argument, () => descriptor, () => defaultValue);
        }
    }
}
