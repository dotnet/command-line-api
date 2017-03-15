// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class Accept
    {
        public static ArgumentsRule AnyOneOf(params string[] values) =>
            ExactlyOneArgument()
                .And(
                    new ArgumentsRule(o => !values.Contains(
                                               o.Arguments.Single(),
                                               StringComparer.OrdinalIgnoreCase)
                                               ? $"Argument '{o.Arguments.Single()}' not recognized. Must be one of:\n\t{string.Join("\n\t", values.Select(v => $"'{v}'"))}"
                                               : "", values));

        public static ArgumentsRule AnyOneOf(
            Func<IEnumerable<string>> getValues) =>
            ExactlyOneArgument()
                .And(
                    new ArgumentsRule(o =>
                    {
                        var values = getValues().ToArray();

                        return !values
                                   .Contains(
                                       o.Arguments.Single(),
                                       StringComparer.OrdinalIgnoreCase)
                                   ? $"Argument '{o.Arguments.Single()}' not recognized. Must be one of:\n\t{string.Join("\n\t", values.Select(v => $"'{v}'"))}"
                                   : "";
                    }, null))
                .WithSuggestionsFrom(_ => getValues());

        public static ArgumentsRule ExactlyOneArgument(
            Func<AppliedOption, string> errorMessage = null) =>
            new ArgumentsRule(o =>
                              {
                                  var argumentCount = o.Arguments.Count;

                                  if (argumentCount == 0)
                                  {
                                      if (errorMessage == null)
                                      {
                                          return $"Required argument missing for {(o.Option.IsCommand ? "command" : "option")}: {o.Option}";
                                      }
                                      else
                                      {
                                          return errorMessage(o);
                                      }
                                  }

                                  if (argumentCount > 1)
                                  {
                                      if (errorMessage == null)
                                      {
                                          return $"{(o.Option.IsCommand ? "Command" : "Option")} '{o.Option}' only accepts a single argument but {argumentCount} were provided.";
                                      }
                                      else
                                      {
                                          return errorMessage(o);
                                      }
                                  }

                                  return null;
                              },
                              materialize: o => o.Arguments.Single());

        public static ArgumentsRule ExistingFilesOnly(
            this ArgumentsRule rule) =>
            rule.And(new ArgumentsRule(o =>
            {
                foreach (var filePath in o.Arguments)
                {
                    if (!File.Exists(filePath) &&
                        !Directory.Exists(filePath))
                    {
                        return $"File does not exist: {filePath}";
                    }
                }

                return null;
            }));

        public static ArgumentsRule WithSuggestionsFrom(
            params string[] values) =>
            new ArgumentsRule(
                _ => null,
                suggest: parseResult => values.FindSuggestions(parseResult.TextToMatch()));

        public static ArgumentsRule WithSuggestionsFrom(
            Func<string, IEnumerable<string>> suggest) =>
            new ArgumentsRule(
                _ => null,
                suggest: parseResult => suggest(parseResult.TextToMatch()));

        public static ArgumentsRule WithSuggestionsFrom(
            this ArgumentsRule rule,
            Func<string, IEnumerable<string>> suggest) =>
            rule.And(WithSuggestionsFrom(suggest));

        public static ArgumentsRule WithSuggestionsFrom(
            this ArgumentsRule rule,
            params string[] values) =>
            rule.And(WithSuggestionsFrom(values));

        public static ArgumentsRule ZeroOrOneArgument() =>
            new ArgumentsRule(o =>
                              {
                                  if (o.Arguments.Count > 1)
                                  {
                                      return $"{(o.Option.IsCommand ? "Command" : "Option")} '{o.Option}' only accepts a single argument but {o.Arguments.Count} were provided.";
                                  }

                                  return null;
                              },
                              materialize: o => o.Arguments.SingleOrDefault());

        internal static ArgumentsRule ExactlyOneCommandRequired(
            Func<AppliedOption, string> errorMessage = null) =>
            new ArgumentsRule(o =>
            {
                var optionCount = o.AppliedOptions.Count;

                if (optionCount == 0)
                {
                    if (errorMessage == null)
                    {
                        return $"Required option missing for command: {o.Option}";
                    }
                    else
                    {
                        return errorMessage(o);
                    }
                }

                if (optionCount > 1)
                {
                    if (errorMessage == null)
                    {
                        return
                            $"Command '{o.Option}' only accepts a single subcommand but {optionCount} were provided: {string.Join(", ", o.AppliedOptions.Select(a => a.Option))}";
                    }
                    else
                    {
                        return errorMessage(o);
                    }
                }

                return null;
            });

        public static ArgumentsRule NoArguments(
            Func<AppliedOption, string> errorMessage = null) =>
            new ArgumentsRule(o =>
                              {
                                  if (!o.Arguments.Any())
                                  {
                                      return null;
                                  }

                                  if (errorMessage == null)
                                  {
                                      return $"Arguments not allowed for option: {o.Option}";
                                  }
                                  else
                                  {
                                      return errorMessage(o);
                                  }
                              },
                              materialize: _ => true);

        public static ArgumentsRule OneOrMoreArguments(
            Func<AppliedOption, string> errorMessage = null) =>
            new ArgumentsRule(o =>
            {
                var optionCount = o.Arguments.Count;

                if (optionCount == 0)
                {
                    if (errorMessage == null)
                    {
                        return $"Required argument missing for {(o.Option.IsCommand ? "command" : "option")}: {o.Option}";
                    }
                    else
                    {
                        return errorMessage(o);
                    }
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
                new ArgumentsRule(
                    o =>
                    {
                        var unrecognized = values
                            .Where(v => !o.Option
                                          .DefinedOptions
                                          .Any(oo => oo.HasAlias(v)))
                            .ToArray();

                        return unrecognized.Any()
                                   ? $"Options '{string.Join(", ", unrecognized)}' not recognized. Must be one of:\n\t{string.Join(Environment.NewLine + "\t", values.Select(v => $"'{v}'"))}"
                                   : null;
                    },
                    completionValues);
        }

        public static ArgumentsRule ZeroOrMoreArguments() =>
            new ArgumentsRule(_ => null,
                              materialize: o => o.Arguments);
    }
}