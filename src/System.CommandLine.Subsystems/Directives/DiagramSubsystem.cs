// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.Text;
using System.CommandLine.Parsing;

namespace System.CommandLine.Directives;

public class DiagramSubsystem(IAnnotationProvider? annotationProvider = null)
    : DirectiveSubsystem("diagram", SubsystemKind.Diagram, annotationProvider)
{
    //protected internal override bool GetIsActivated(ParseResult? parseResult)
    //   => parseResult is not null && option is not null && parseResult.GetValue(option);

    protected internal override void Execute(PipelineResult pipelineResult)
    {
        // Gather locations
        //var locations = pipelineResult.ParseResult.LocationMap
        //                   .Concat(Map(pipelineResult.ParseResult.Configuration.PreProcessedLocations));

        pipelineResult.ConsoleHack.WriteLine("Output diagram");
        pipelineResult.SetSuccess();
    }


    // TODO: Capture logic in previous diagramming, shown below
    /// <summary>
    /// Formats a string explaining a parse result.
    /// </summary>
    /// <param name="parseResult">The parse result to be diagrammed.</param>
    /// <returns>A string containing a diagram of the parse result.</returns>
    internal static StringBuilder Diagram(ParseResult parseResult)
    {
        var builder = new StringBuilder(100);


        // TODO: Reinstate this when ready to implement
        //Diagram(builder, parseResult.RootCommandResult, parseResult);

        // TODO: Unmatched tokens
        /*
        var unmatchedTokens = parseResult.UnmatchedTokens;
        if (unmatchedTokens.Count > 0)
        {
            builder.Append("   ???-->");

            for (var i = 0; i < unmatchedTokens.Count; i++)
            {
                var error = unmatchedTokens[i];
                builder.Append(' ');
                builder.Append(error);
            }
        }
        */

        return builder;
    }

    /*
    private static void Diagram(
        StringBuilder builder,
        SymbolResult symbolResult,
        ParseResult parseResult)
    {
        if (parseResult.Errors.Any(e => e.SymbolResult == symbolResult))
        {
            builder.Append('!');
        }
    */
    // TODO: Directives
    /*
        switch (symbolResult)
        {
            case DirectiveResult { Directive: not DiagramDirective }:
                break;
            */

    // TODO: This logic is deeply tied to internal types/properties. These aren't things we probably want to expose like SymbolNode. See #2349 for alternatives
    /*
    case ArgumentResult argumentResult:
        {
            var includeArgumentName =
                argumentResult.Argument.FirstParent!.Symbol is CliCommand { HasArguments: true, Arguments.Count: > 1 };

            if (includeArgumentName)
            {
                builder.Append("[ ");
                builder.Append(argumentResult.Argument.Name);
                builder.Append(' ');
            }

            if (argumentResult.Argument.Arity.MaximumNumberOfValues > 0)
            {
                ArgumentConversionResult conversionResult = argumentResult.GetArgumentConversionResult();
                switch (conversionResult.Result)
                {
                    case ArgumentConversionResultType.NoArgument:
                        break;
                    case ArgumentConversionResultType.Successful:
                        switch (conversionResult.Value)
                        {
                            case string s:
                                builder.Append($"<{s}>");
                                break;

                            case IEnumerable items:
                                builder.Append('<');
                                builder.Append(
                                    string.Join("> <",
                                                items.Cast<object>().ToArray()));
                                builder.Append('>');
                                break;

                            default:
                                builder.Append('<');
                                builder.Append(conversionResult.Value);
                                builder.Append('>');
                                break;
                        }

                        break;

                    default: // failures
                        builder.Append('<');
                        builder.Append(string.Join("> <", symbolResult.Tokens.Select(t => t.Value)));
                        builder.Append('>');

                        break;
                }
            }

            if (includeArgumentName)
            {
                builder.Append(" ]");
            }

            break;
        }

    default:
        {
            OptionResult? optionResult = symbolResult as OptionResult;

            if (optionResult is { Implicit: true })
            {
                builder.Append('*');
            }

            builder.Append("[ ");

            if (optionResult is not null)
            {
                builder.Append(optionResult.IdentifierToken?.Value ?? optionResult.Option.Name);
            }
            else
            {
                builder.Append(((CommandResult)symbolResult).IdentifierToken.Value);
            }

            foreach (SymbolResult child in symbolResult.SymbolResultTree.GetChildren(symbolResult))
            {
                if (child is ArgumentResult arg &&
                    (arg.Argument.ValueType == typeof(bool) ||
                     arg.Argument.Arity.MaximumNumberOfValues == 0))
                {
                    continue;
                }

                builder.Append(' ');

                Diagram(builder, child, parseResult);
            }

            builder.Append(" ]");
            break;
        }
    }
}
}
*/
}
