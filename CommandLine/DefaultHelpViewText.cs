// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class DefaultHelpViewText
    {
        public static string AdditionalArgumentsSection { get; set; } =
            $"{Environment.NewLine}Additional Arguments:{Environment.NewLine}  Arguments passed to the application that is being run.";

        public static class ArgumentsSection
        {
            public static string Title { get; set; } = "Arguments:";
        }

        public static class CommandsSection
        {
            public static string Title { get; set; } = "Commands:";
        }

        public static class OptionsSection
        {
            public static string Title { get; set; } = "Options:";
        }

        public static class Synopsis
        {
            public static string AdditionalArguments { get; set; } = " [[--] <additional arguments>...]]";
            public static string Command { get; set; } = " [command]";
            public static string Options { get; set; } = " [options]";
            public static string Title { get; set; } = "Usage:";
        }
    }
}
