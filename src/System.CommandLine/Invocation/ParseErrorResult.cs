// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;

namespace System.CommandLine.Invocation
{
    public class ParseErrorResult : IInvocationResult
    {
        private readonly int? _errorExitCode;

        public ParseErrorResult(int? errorExitCode)
        {
            _errorExitCode = errorExitCode;
        }

        public void Apply(InvocationContext context)
        {
            context.Console.ResetTerminalForegroundColor();
            context.Console.SetTerminalForegroundRed();

            foreach (var error in context.ParseResult.Errors)
            {
                context.Console.Error.WriteLine(error.Message);
            }

            context.Console.Error.WriteLine();

            context.ExitCode = _errorExitCode ?? 1;

            context.Console.ResetTerminalForegroundColor();

            context.BindingContext
                   .HelpBuilder
                   .Write(context.ParseResult.CommandResult.Command);
        }
    }
}
