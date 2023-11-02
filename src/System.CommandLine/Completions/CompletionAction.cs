// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Completions;

/// <summary>
/// Implements the action for the <c>[suggest]</c> directive, which when specified on the command line will short circuit normal command handling and display a diagram explaining the parse result for the command line input.
/// </summary>
internal sealed class CompletionAction : SynchronousCliAction
{
    private readonly SuggestDirective _directive;

    internal CompletionAction(SuggestDirective suggestDirective) => _directive = suggestDirective;

    /// <inheritdoc />
    public override int Invoke(ParseResult parseResult)
    {
        string? parsedValues = parseResult.GetResult(_directive)!.Values.SingleOrDefault();
        string? rawInput = parseResult.CommandLineText;

        int position = !string.IsNullOrEmpty(parsedValues) ? int.Parse(parsedValues) : rawInput?.Length ?? 0;

        var commandLineToComplete = parseResult.Tokens.LastOrDefault(t => t.Type != CliTokenType.Directive)?.Value ?? "";

        var completionParseResult = parseResult.RootCommandResult.Command.Parse(commandLineToComplete, parseResult.Configuration);

        var completions = completionParseResult.GetCompletions(position);

        parseResult.Configuration.Output.WriteLine(
            string.Join(
                Environment.NewLine,
                completions));

        return 0;
    }
}