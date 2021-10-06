// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.IO;

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
            helpBuilder.Customize(option, (_) => descriptor, (_) => defaultValue);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            ICommand command,
            string? descriptor = null)
        {
            helpBuilder.Customize(command, (_) => descriptor);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            IArgument argument,
            string? descriptor = null,
            string? defaultValue = null)
        {
            helpBuilder.Customize(argument, (_) => descriptor, (_) => defaultValue);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            IOption option,
            Func<ParseResult?, string?>? descriptor = null,
            Func<ParseResult?, string?>? defaultValue = null)
        {
            helpBuilder.Customize(option, descriptor, defaultValue);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            ICommand command,
            Func<ParseResult?, string?>? descriptor = null)
        {
            helpBuilder.Customize(command, descriptor);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            IArgument argument,
            Func<ParseResult?, string?>? descriptor = null,
            Func<ParseResult?, string?>? defaultValue = null)
        {
            helpBuilder.Customize(argument, descriptor, defaultValue);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            IOption option)
        {
            helpBuilder.Customize(option);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            ICommand command)
        {
            helpBuilder.Customize(command);
        }

        public static void Customize(this HelpBuilder helpBuilder,
            IArgument argument)
        {
            helpBuilder.Customize(argument);
        }

        public static void Write(this IHelpBuilder builder, ICommand command, TextWriter writer) =>
            builder.Write(command, writer, ParseResult.Empty);
    }
}
