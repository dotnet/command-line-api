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
            Func<AppliedOption, string> errorMessage = null) =>
            new ArgumentsRule(o =>
                              {
                                  var argumentCount = o.Arguments.Count;

                                  if (argumentCount == 0)
                                  {
                                      if (errorMessage == null)
                                      {
                                          return o.Option.IsCommand
                                                     ? RequiredArgumentMissingForCommand(o.Option.ToString())
                                                     : RequiredArgumentMissingForOption(o.Option.ToString());
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
                                          return o.Option.IsCommand
                                                     ? CommandAcceptsOnlyOneArgument(o.Option.ToString(), argumentCount)
                                                     : OptionAcceptsOnlyOneArgument(o.Option.ToString(), argumentCount);
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
                                      return o.Option.IsCommand
                                                 ? CommandAcceptsOnlyOneArgument(o.Option.ToString(), o.Arguments.Count)
                                                 : OptionAcceptsOnlyOneArgument(o.Option.ToString(), o.Arguments.Count);
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
                        return RequiredArgumentMissingForCommand(o.Option.ToString());
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
                            o.Option.ToString(),
                            string.Join(", ", o.AppliedOptions.Select(a => a.Option)));
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
                                      return NoArgumentsAllowed(o.Option.ToString());
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
                                          return
                                              o.Option.IsCommand
                                                  ? RequiredArgumentMissingForCommand(o.Option.ToString())
                                                  : RequiredArgumentMissingForOption(o.Option.ToString());
                                      }
                                      else
                                      {
                                          return errorMessage(o);
                                      }
                                  }

                                  return null;
                              },
                              materialize: o => o.Arguments);

        internal static ArgumentsRule ZeroOrMoreOf(params Option[] options)
        {
            var values = options
                .SelectMany(o => o.RawAliases)
                .ToArray();

            var completionValues = options
                .Where(o => !o.IsHidden())
                .SelectMany(o => o.RawAliases)
                .ToArray();

            return
                new ArgumentsRule(
                    o =>
                    {
                        var unrecognized = values
                            .FirstOrDefault(v => !o.Option
                                                   .DefinedOptions
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
