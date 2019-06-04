// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal abstract class SyntaxVisitor
    {
        public void Visit(SyntaxNode node)
        {
            Start(node);

            VisitInternal(node);

            Stop(node);
        }

        private void VisitInternal(SyntaxNode node)
        {
            switch (node)
            {
                case DirectiveNode directiveNode:
                    VisitDirectiveNode(directiveNode);

                    break;

                case RootCommandNode rootCommandNode:
                    VisitRootCommandNode(rootCommandNode);

                    foreach (var child in rootCommandNode.Children)
                    {
                        VisitInternal(child);
                    }

                    break;

                case CommandNode commandNode:
                    VisitCommandNode(commandNode);

                    foreach (var child in commandNode.Children)
                    {
                        VisitInternal(child);
                    }

                    break;

                case OptionNode optionNode:
                    VisitOptionNode(optionNode);

                    foreach (var child in optionNode.Children)
                    {
                        VisitInternal(child);
                    }

                    break;

                case CommandArgumentNode commandArgumentNode:
                    VisitCommandArgumentNode(commandArgumentNode);

                    break;

                case OptionArgumentNode optionArgumentNode:
                    VisitOptionArgumentNode(optionArgumentNode);

                    break;

                default:
                    VisitUnknownNode(node);

                    break;
            }
        }

        protected virtual void VisitCommandArgumentNode(CommandArgumentNode argumentNode)
        {
        }

        protected virtual void VisitOptionArgumentNode(OptionArgumentNode argumentNode)
        {
        }

        protected virtual void VisitOptionNode(OptionNode optionNode)
        {
        }

        protected virtual void VisitRootCommandNode(RootCommandNode rootCommandNode)
        {
        }

        protected virtual void VisitCommandNode(CommandNode commandNode)
        {
        }

        protected virtual void VisitDirectiveNode(DirectiveNode directiveNode)
        {
        }

        protected virtual void Start(SyntaxNode node)
        {
        }

        protected virtual void Stop(SyntaxNode node)
        {
        }

        protected virtual void VisitUnknownNode(SyntaxNode node)
        {
        }
    }
}
