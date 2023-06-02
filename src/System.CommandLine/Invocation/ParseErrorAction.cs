// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;

namespace System.CommandLine.Invocation;

internal sealed class ParseErrorAction : SynchronousCliAction
{
    public override int Invoke(ParseResult parseResult)
    {
        ConsoleHelpers.ResetTerminalForegroundColor();
        ConsoleHelpers.SetTerminalForegroundRed();

        foreach (var error in parseResult.Errors)
        {
            parseResult.Configuration.Error.WriteLine(error.Message);
        }

        parseResult.Configuration.Error.WriteLine();

        ConsoleHelpers.ResetTerminalForegroundColor();

        new HelpAction().Invoke(parseResult);

        return 1;
    }
}