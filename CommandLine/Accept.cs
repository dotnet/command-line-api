// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class Accept
    {
        public static ArgumentsRule AnyOneOf(params string[] values) =>
            ExactlyOneArgument
                .And(
                    ParseRule(o => !values.Contains(
                                       o.Arguments.Single(),
                                       StringComparer.OrdinalIgnoreCase)
                                       ? $"Argument '{o.Arguments.Single()}' not recognized. Must be one of:\n\t{string.Join("\n\t", values.Select(v => $"'{v}'"))}"
                                       : "",
                              values));

        public static ArgumentsRule AnyOneOf(
            Func<IEnumerable<string>> getValues) =>
            ExactlyOneArgument
                .And(
                    ParseRule(o =>
                    {
                        var values = getValues().ToArray();

                        return !values
                                   .Contains(
                                       o.Arguments.Single(),
                                       StringComparer.OrdinalIgnoreCase)
                                   ? $"Argument '{o.Arguments.Single()}' not recognized. Must be one of:\n\t{string.Join("\n\t", values.Select(v => $"'{v}'"))}"
                                   : "";
                    }))
                .WithSuggestionsFrom(_ => getValues());

        public static ArgumentsRule ExactlyOneArgument { get; } =
            new ArgumentsRule(o =>
            {
                var argumentCount = o.Arguments.Count;

                if (argumentCount == 0)
                {
                    var optionType = o.Option.IsCommand ? "command" : "option";
                    return $"Required argument missing for {optionType}: {o.Option}";
                }

                if (argumentCount > 1)
                {
                    var optionType = o.Option.IsCommand ? "Command" : "Option";
                    return $"{optionType} '{o.Option}' only accepts a single argument but {argumentCount} were provided.";
                }

                return null;
            });

        public static ArgumentsRule WithSuggestionsFrom(
            params string[] values) =>
            new ArgumentsRule(
                _ => null,
                suggest: parseResult =>  values.FindSuggestions(parseResult.TextToMatch()));

        public static ArgumentsRule WithSuggestionsFrom(
            Func<string, IEnumerable<string>> suggest) =>
            new ArgumentsRule(
                _ => null,
                suggest: parseResult => suggest(parseResult.TextToMatch()));

        public static ArgumentsRule WithSuggestionsFrom(
            this ArgumentsRule rule,
            Func<string, IEnumerable<string>> suggest) =>
            rule.And(WithSuggestionsFrom(suggest));

        public static ArgumentsRule ZeroOrOneArgument { get; } =
            new ArgumentsRule(o =>
            {
                if (o.Arguments.Count > 1)
                {
                    var optionType = o.Option.IsCommand ? "Command" : "Option";
                    return $"{optionType} '{o.Option}' only accepts a single argument but {o.Arguments.Count} were provided.";
                }

                return null;
            });

        internal static ArgumentsRule ExactlyOneCommandRequired { get; } =
            new ArgumentsRule(o =>
            {
                var optionCount = o.AppliedOptions.Count;

                if (optionCount == 0)
                {
                    return $"Required option missing for command: {o.Option}";
                }

                if (optionCount > 1)
                {
                    return $"Command '{o.Option}' only accepts a single subcommand but {optionCount} were provided: {string.Join(", ", o.AppliedOptions.Select(a => a.Option))}";
                }

                return null;
            });

        public static ArgumentsRule NoArguments { get; } =
            new ArgumentsRule(o =>
                                  o.Arguments.Any()
                                      ? $"Arguments not allowed for option: {o.Option}"
                                      : null);

        public static ArgumentsRule OneOrMoreArguments { get; } =
            new ArgumentsRule(o =>
            {
                var optionCount = o.Arguments.Count;

                if (optionCount == 0)
                {
                    var optionType = o.Option.IsCommand ? "command" : "option";
                    return $"Required argument missing for {optionType}: {o.Option}";
                }

                return null;
            });

        public static ArgumentsRule ZeroOrMoreOf(params Option[] options)
        {
            var values = options
                .SelectMany(o =>
                                o.IsCommand
                                    ? o.Aliases
                                    : o.Aliases.Select(v => v.AddPrefix()))
                .ToArray();

            var completionValues = options
                .Where(o => !o.IsHidden())
                .SelectMany(o =>
                                o.IsCommand
                                    ? o.Aliases
                                    : o.Aliases.Select(v => v.AddPrefix()))
                .ToArray();

            return
                ParseRule(o =>
                          {
                              var unrecognized = values
                                  .Where(v => !o.Option
                                                .DefinedOptions
                                                .Any(oo => oo.HasAlias(v)))
                                  .ToArray();

                              return unrecognized.Any()
                                         ? $"Options '{string.Join(", ", unrecognized)}' not recognized. Must be one of:\n\t{string.Join(Environment.NewLine + "\t", values.Select(v => $"'{v}'"))}"
                                         : "";
                          },
                          completionValues);
        }

        public static ArgumentsRule ZeroOrMoreArguments { get; } =
            new ArgumentsRule(_ => null);
    }
}