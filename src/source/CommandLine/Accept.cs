// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Microsoft.DotNet.Cli.CommandLine.ValidationMessages;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class Accept
    {
        public static ArgumentsRule AnyOneOf(params string[] values) =>
            ExactlyOneArgument()
                .And(
                    new ArgumentsRule(o =>
                    {
                        var arg = o.Arguments.Single();

                        return !values.Contains(arg, StringComparer.OrdinalIgnoreCase)
                                   ? UnrecognizedArgument(arg, values)
                                   : "";
                    }, values));

        public static ArgumentsRule AnyOneOf(
            Func<IEnumerable<string>> getValues) =>
            ExactlyOneArgument()
                .And(
                    new ArgumentsRule(o =>
                    {
                        var values = getValues().ToArray();

                        var arg = o.Arguments.Single();

                        return !values
                                   .Contains(arg, StringComparer.OrdinalIgnoreCase)
                                   ? UnrecognizedArgument(arg, values)
                                   : "";
                    }, null))
                .WithSuggestionsFrom(_ => getValues());

        public static ArgumentsRule ExactlyOneArgument(
            Func<ParsedSymbol, string> errorMessage = null) =>
            new ArgumentsRule(o =>
                              {
                                  var argumentCount = o.Arguments.Count;

                                  if (argumentCount == 0)
                                  {
                                      if (errorMessage == null)
                                      {
                                          return o.Symbol is Command
                                                     ? RequiredArgumentMissingForCommand(o.Symbol.ToString())
                                                     : RequiredArgumentMissingForOption(o.Symbol.ToString());
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
                                          return o.Symbol is Command
                                                     ? CommandAcceptsOnlyOneArgument(o.Symbol.ToString(), argumentCount)
                                                     : OptionAcceptsOnlyOneArgument(o.Symbol.ToString(), argumentCount);
                                      }
                                      else
                                      {
                                          return errorMessage(o);
                                      }
                                  }

                                  return null;
                              },
                              materialize: o => o.Arguments.SingleOrDefault());

        public static ArgumentsRule ExistingFilesOnly(
            this ArgumentsRule rule) =>
            rule.And(new ArgumentsRule(o => o.Arguments
                                             .Where(filePath => !File.Exists(filePath) &&
                                                                !Directory.Exists(filePath))
                                             .Select(FileDoesNotExist)
                                             .FirstOrDefault()));

        public static ArgumentsRule LegalFilePathsOnly(
            this ArgumentsRule rule) =>
            rule.And(new ArgumentsRule(o =>
            {
                foreach (var arg in o.Arguments)
                {
                    try
                    {
                        var fileInfo = new FileInfo(arg);
                    }
                    catch (NotSupportedException ex)
                    {
                        return ex.Message;
                    }
                    catch (ArgumentException ex)
                    {
                        return ex.Message;
                    }
                }

                return null;
            }));

        public static ArgumentsRule WithSuggestionsFrom(
            params string[] values) =>
            new ArgumentsRule(
                _ => null,
                suggest: (parseResult, position) => values.FindSuggestions(parseResult.TextToMatch(position)));

        public static ArgumentsRule WithSuggestionsFrom(
            Func<string, IEnumerable<string>> suggest) =>
            new ArgumentsRule(
                _ => null,
                suggest: (parseResult, position) => suggest(parseResult.TextToMatch(position)));

        public static ArgumentsRule WithSuggestionsFrom(
            this ArgumentsRule rule,
            Func<string, IEnumerable<string>> suggest) =>
            rule.And(WithSuggestionsFrom(suggest));

        public static ArgumentsRule ZeroOrOneArgument() =>
            new ArgumentsRule(o =>
                              {
                                  if (o.Arguments.Count > 1)
                                  {
                                      return o.Symbol is Command
                                                 ? CommandAcceptsOnlyOneArgument(o.Symbol.ToString(), o.Arguments.Count)
                                                 : OptionAcceptsOnlyOneArgument(o.Symbol.ToString(), o.Arguments.Count);
                                  }

                                  return null;
                              },
                              materialize: o => o.Arguments.SingleOrDefault());

        internal static ArgumentsRule ExactlyOneCommandRequired(
            Func<ParsedSymbol, string> errorMessage = null) =>
            new ArgumentsRule(o =>
            {
                var optionCount = o.Children.Count;

                if (optionCount == 0)
                {
                    if (errorMessage == null)
                    {
                        return RequiredArgumentMissingForCommand(o.Symbol.ToString());
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
                        return CommandAcceptsOnlyOneSubcommand(
                            o.Symbol.ToString(),
                            string.Join(", ", o.Children.Select(a => a.Symbol)));
                    }
                    else
                    {
                        return errorMessage(o);
                    }
                }

                return null;
            });

        public static ArgumentsRule NoArguments(
            Func<ParsedSymbol, string> errorMessage = null) =>
            new ArgumentsRule(o =>
                              {
                                  if (!o.Arguments.Any())
                                  {
                                      return null;
                                  }

                                  if (errorMessage == null)
                                  {
                                      return NoArgumentsAllowed(o.Symbol.ToString());
                                  }
                                  else
                                  {
                                      return errorMessage(o);
                                  }
                              },
                              materialize: _ => true);

        public static ArgumentsRule OneOrMoreArguments(
            Func<ParsedSymbol, string> errorMessage = null) =>
            new ArgumentsRule(o =>
                              {
                                  var optionCount = o.Arguments.Count;

                                  if (optionCount == 0)
                                  {
                                      if (errorMessage == null)
                                      {
                                          return
                                              o.Symbol is Command
                                                  ? RequiredArgumentMissingForCommand(o.Symbol.ToString())
                                                  : RequiredArgumentMissingForOption(o.Symbol.ToString());
                                      }
                                      else
                                      {
                                          return errorMessage(o);
                                      }
                                  }

                                  return null;
                              },
                              materialize: o => o.Arguments);

        internal static ArgumentsRule ZeroOrMoreOf(params Symbol[] symbols)
        {
            var values = symbols
                .SelectMany(o => o.RawAliases)
                .ToArray();

            var completionValues = symbols
                .Where(o => !o.IsHidden())
                .SelectMany(o => o.RawAliases)
                .ToArray();

            return
                new ArgumentsRule(
                    o =>
                    {
                        var unrecognized = values
                            .FirstOrDefault(v => !o.Symbol
                                                   .DefinedSymbols
                                                   .Any(oo => oo.HasAlias(v)));

                        if (unrecognized != null)
                        {
                            return UnrecognizedOption(unrecognized, values);
                        }
                        else
                        {
                            return null;
                        }
                    },
                    completionValues);
        }

        public static ArgumentsRule ZeroOrMoreArguments() =>
            new ArgumentsRule(_ => null,
                              materialize: o => o.Arguments);
    }
}
