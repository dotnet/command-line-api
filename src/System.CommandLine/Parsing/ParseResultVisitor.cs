// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal class ParseResultVisitor : SyntaxVisitor
    {
        private readonly Parser _parser;
        private readonly TokenizeResult _tokenizeResult;
        private readonly string _rawInput;

        private readonly DirectiveCollection _directives = new DirectiveCollection();
        private readonly List<string> _unparsedTokens = new List<string>();
        private readonly List<string> _unmatchedTokens = new List<string>();
        private readonly List<TokenizeError> _errors;

        private  CommandResult _rootCommandResult;
        private CommandResult _innermostCommandResult;

        public ParseResultVisitor(
            Parser parser,
            TokenizeResult tokenizeResult,
            string rawInput)
        {
            _parser = parser;
            _tokenizeResult = tokenizeResult;
            _rawInput = rawInput;
            _errors = new List<TokenizeError>(_tokenizeResult.Errors);
        }

        protected override void VisitRootCommandNode(RootCommandNode rootCommandNode)
        {
            _rootCommandResult = new CommandResult(rootCommandNode.Command);
            _innermostCommandResult = _rootCommandResult;
        }

        protected override void VisitCommandNode(CommandNode commandNode)
        {
            _innermostCommandResult = new CommandResult(commandNode.Command, commandNode.Token, _innermostCommandResult);
        }

        protected override void VisitDirectiveNode(DirectiveNode directiveNode)
        {
            _directives.Add(directiveNode.Name, directiveNode.Value);
        }

        protected override void VisitUnknownNode(SyntaxNode node)
        {
            _unmatchedTokens.Add(node.Token.Value);
        }

        public ParseResult Result =>
            new ParseResult(
                _parser,
                _rootCommandResult,
                _innermostCommandResult,
                _directives,
                _tokenizeResult.Tokens,
                _unparsedTokens,
                _unmatchedTokens,
                _errors,
                _rawInput
            );
    }
}
