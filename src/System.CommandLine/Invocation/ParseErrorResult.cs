// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class ParseErrorResult : CliAction
    {
        public override int Invoke(InvocationContext context)
            => Apply(context);

        public override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
            => cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<int>(cancellationToken)
                : Task.FromResult(Apply(context));

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

            HelpOption.Display(context);

            return context.ParseResult.Configuration.ParseErrorReportingExitCode!.Value;
        }
    }
}