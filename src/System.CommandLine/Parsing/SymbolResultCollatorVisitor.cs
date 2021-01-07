// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal class SymbolResultCollatorVisitor : SymbolResultVisitor
    {
        public Dictionary<IArgument, ArgumentResult> ArgumentResults { get; } = new Dictionary<IArgument, ArgumentResult>();

        public Dictionary<ICommand, CommandResult> CommandResults { get; } = new Dictionary<ICommand, CommandResult>();

        public Dictionary<IOption, OptionResult> OptionResults { get; } = new Dictionary<IOption, OptionResult>();

        protected override void VisitOptionResult(OptionResult optionResult)
        {
            OptionResults.Add(optionResult.Option, optionResult);
        }

        protected override void VisitCommandResult(CommandResult commandResult)
        {
            CommandResults.Add(commandResult.Command, commandResult);
        }

        protected override void VisitRootCommandResult(RootCommandResult rootCommandResult)
        {
            CommandResults.Add(rootCommandResult.Command, rootCommandResult);
        }

        protected override void VisitArgumentResult(ArgumentResult argumentResult)
        {
            ArgumentResults.Add(argumentResult.Argument, argumentResult);
        }
    }
}