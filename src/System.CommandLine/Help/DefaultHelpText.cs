// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace System.CommandLine
{
    /// <summary>
    /// Static strings used for default titles and labels in the output help sections
    /// </summary>
    public static class DefaultHelpText
    {
        public static class AdditionalArguments
        {
            public static string Title { get; set; } = "Additional Arguments:";
            public static string Description { get; set; } = "Arguments passed to the application that is being run.";
        }

        public static class Arguments
        {
            public static string Title { get; set; } = "Arguments:";
        }

        public static class Commands
        {
            public static string Title { get; set; } = "Commands:";
        }

        public static class Options
        {
            public static string Title { get; set; } = "Options:";
        }

        public static class Usage
        {
            public static string AdditionalArguments { get; set; } = "[[--] <additional arguments>...]]";
            public static string Command { get; set; } = "[command]";
            public static string Options { get; set; } = "[options]";
            public static string Title { get; set; } = "Usage:";
        }
    }
}
