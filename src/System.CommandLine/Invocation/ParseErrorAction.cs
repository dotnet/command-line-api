// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class ParseErrorAction : CliAction
    {
        protected override int Invoke(ParseResult parseResult)
        {
            ConsoleHelpers.ResetTerminalForegroundColor();
            ConsoleHelpers.SetTerminalForegroundRed();

            foreach (var error in parseResult.Errors)
            {
                parseResult.Configuration.Error.WriteLine(error.Message);
            }

            parseResult.Configuration.Error.WriteLine();

            ConsoleHelpers.ResetTerminalForegroundColor();

            var helpBuilder = new HelpAction().Builder;

            var helpContext = new HelpContext(helpBuilder,
                parseResult.CommandResult.Command,
                parseResult.Configuration.Output,
                parseResult);

            helpBuilder.Write(helpContext);

            return 1;
        }

        protected override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(Invoke(parseResult));
    }
}