﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.IO;

namespace System.CommandLine.Invocation
{
    internal static class ParseErrorResult
    {
        internal static int Apply(InvocationContext context)
        {
            context.Console.ResetTerminalForegroundColor();
            context.Console.SetTerminalForegroundRed();

            foreach (var error in context.ParseResult.Errors)
            {
                context.Console.Error.WriteLine(error.Message);
            }

            context.Console.Error.WriteLine();

            context.Console.ResetTerminalForegroundColor();

            HelpOption.Handler(context);

            return context.ParseResult.Configuration.ParseErrorReportingExitCode!.Value;
        }
    }
}