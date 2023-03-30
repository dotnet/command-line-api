﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class ParseErrorResultAction : CliAction
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

            new HelpOption().Action!.Invoke(parseResult);

            return 1;
        }

        public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(Invoke(parseResult));
    }
}