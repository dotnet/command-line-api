// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class ParseErrorResultAction : CliAction
    {
        public override int Invoke(InvocationContext context)
        {
            ConsoleHelpers.ResetTerminalForegroundColor();
            ConsoleHelpers.SetTerminalForegroundRed();

            foreach (var error in context.ParseResult.Errors)
            {
                context.ParseResult.Configuration.Error.WriteLine(error.Message);
            }

            context.ParseResult.Configuration.Error.WriteLine();

            ConsoleHelpers.ResetTerminalForegroundColor();

            new HelpOption().Action!.Invoke(context);

            return 1;
        }

        public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(Invoke(context));
    }
}