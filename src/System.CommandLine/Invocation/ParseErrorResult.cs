// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.IO;
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
                context.Console.Error.WriteLine(error.Message);
            }

            context.Console.Error.WriteLine();

            ConsoleHelpers.ResetTerminalForegroundColor();

            new HelpOption().Action!.Invoke(context);

            return context.ParseResult.Configuration.ParseErrorReportingExitCode!.Value;
        }

        public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(Invoke(context));
    }
}