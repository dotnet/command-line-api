// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal abstract class SymbolResultVisitor
    {
        public void Visit(SymbolResult symbolResult)
        {
            Start(symbolResult);

            VisitInternal(symbolResult);

            Stop(symbolResult);
        }

        private void VisitInternal(SymbolResult node)
        {
            switch (node)
            {
                case ArgumentResult argumentResult:
                    VisitArgumentResult(argumentResult);

                    break;

                case RootCommandResult rootCommandResult:
                    VisitRootCommandResult(rootCommandResult);

                    for (var i = 0; i < rootCommandResult.Children.Count; i++)
                    {
                        VisitInternal(rootCommandResult.Children[i]);
                    }

                    break;

                case CommandResult commandResult:
                    VisitCommandResult(commandResult);

                    for (var i = 0; i < commandResult.Children.Count; i++)
                    {
                        VisitInternal(commandResult.Children[i]);
                    }

                    break;

                case OptionResult optionResult:
                    VisitOptionResult(optionResult);

                    for (var i = 0; i < optionResult.Children.Count; i++)
                    {
                        VisitInternal(optionResult.Children[i]);
                    }

                    break;
            }
        }

        protected virtual void VisitOptionResult(OptionResult optionResult)
        {
        }

        protected virtual void VisitCommandResult(CommandResult commandResult)
        {
        }

        protected virtual void VisitArgumentResult(ArgumentResult argumentResult)
        {
        }

        protected virtual void VisitRootCommandResult(RootCommandResult rootCommandResult)
        {
        }

        protected virtual void Start(SymbolResult node)
        {
        }

        protected virtual void Stop(SymbolResult node)
        {
        }
    }
}