// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public class ParseErrorResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            var terminal = context.Console as ITerminal;
            
            if (terminal != null)
            {
                terminal.ResetColor();
                terminal.ForegroundColor = ConsoleColor.Red;
            }

            foreach (var error in context.ParseResult.Errors)
            {
                context.Console.Error.WriteLine(error.Message);
            }

            context.Console.Error.WriteLine();

            context.ResultCode = 1;

            terminal?.ResetColor();

            context.ParseResult.CommandResult.Command.WriteHelp(context.Console);
        }
    }
}
