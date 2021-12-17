// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Help;

public partial class HelpBuilder
{
    /// <summary>
    /// Provides default formatting for help output.
    /// </summary>
    public static class Default
    {
        /// <summary>
        /// Gets an argument's default value to be displayed in help.
        /// </summary>
        /// <param name="argument">The argument to get the default value for.</param>
        public static string GetArgumentDefaultValue(IArgument argument)
        {
            if (argument.HasDefaultValue)
            {
                if (argument.GetDefaultValue() is { } defaultValue)
                {
                    if (defaultValue is IEnumerable enumerable and not string)
                    {
                        return string.Join("|", enumerable.OfType<object>().ToArray());
                    }
                    else
                    {
                        return defaultValue.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the description for an argument (typically used in the second column text in the arguments section).
        /// </summary>
        public static string GetArgumentDescription(IArgument argument) => argument.Description ?? string.Empty;

        /// <summary>
        /// Gets the usage title for an argument (for example: <c>&lt;value&gt;</c>, typically used in the first column text in the arguments usage section, or within the synopsis.
        /// </summary>
        public static string GetArgumentUsageLabel(IArgument argument)
        {
            if (argument.ValueType == typeof(bool) ||
                argument.ValueType == typeof(bool?))
            {
                if (argument.Parents.FirstOrDefault() is ICommand)
                {
                    return $"<{argument.Name}>";
                }
                else
                {
                    return "";
                }
            }

            string firstColumn;
            var completions = (argument is Argument a
                                   ? a.GetCompletions()
                                   : Array.Empty<CompletionItem>())
                              .Select(item=>item.Label)
                              .ToArray();

            var arg = argument as Argument;
            var helpName = arg?.HelpName ?? string.Empty;

            if (!string.IsNullOrEmpty(helpName))
            {
                firstColumn = helpName!;
            }
            else if (completions.Length > 0)
            {
                firstColumn = string.Join("|", completions);
            }
            else
            {
                firstColumn = argument.Name;
            }

            if (!string.IsNullOrWhiteSpace(firstColumn))
            {
                return $"<{firstColumn}>";
            }
            return firstColumn;
        }

        /// <summary>
        /// Gets the description for the specified symbol (typically the used as the second column in help text).
        /// </summary>
        /// <param name="symbol">The symbol to get the description for.</param>
        public static string GetIdentifierSymbolDescription(IIdentifierSymbol symbol) => symbol.Description ?? string.Empty;

        /// <summary>
        /// Gets the usage label for the specified symbol (typically used as the first column text in help output).
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <param name="context">The help context, used for localization purposes.</param>
        /// <returns>Text to display.</returns>
        public static string GetIdentifierSymbolUsageLabel(IIdentifierSymbol symbol, HelpContext context)
        {
            var aliases = symbol.Aliases
                                .Select(r => r.SplitPrefix())
                                .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                                .GroupBy(t => t.Alias)
                                .Select(t => t.First())
                                .Select(t => $"{t.Prefix}{t.Alias}");

            var firstColumnText = string.Join(", ", aliases);

            foreach (var argument in symbol.Arguments())
            {
                if (!argument.IsHidden)
                {
                    var argumentFirstColumnText = Default.GetArgumentUsageLabel(argument);

                    if (!string.IsNullOrWhiteSpace(argumentFirstColumnText))
                    {
                        firstColumnText += $" {argumentFirstColumnText}";
                    }
                }
            }

            if (symbol is IOption { IsRequired: true })
            {
                firstColumnText += $" {context.HelpBuilder.LocalizationResources.HelpOptionsRequiredLabel()}";
            }

            return firstColumnText;
        }

        /// <summary>
        /// Gets the default sections to be written for command line help.
        /// </summary>
        public static IEnumerable<HelpSectionDelegate> GetLayout()
        {
            yield return SynopsisSection();
            yield return CommandUsageSection();
            yield return CommandArgumentsSection();
            yield return OptionsSection();
            yield return SubcommandsSection();
            yield return AdditionalArgumentsSection();
        }

        /// <summary>
        /// Writes a help section describing a command's synopsis.
        /// </summary>
        public static HelpSectionDelegate SynopsisSection() =>
            ctx =>
            {
                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpDescriptionTitle(), ctx.Command.Description, ctx.Output);
            };

        /// <summary>
        /// Writes a help section describing a command's usage.
        /// </summary>
        public static HelpSectionDelegate CommandUsageSection() =>
            ctx =>
            {
                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpUsageTitle(), ctx.HelpBuilder.GetUsage(ctx.Command), ctx.Output);
            };

        ///  <summary>
        /// Writes a help section describing a command's arguments.
        ///  </summary>
        public static HelpSectionDelegate CommandArgumentsSection() =>
            ctx =>
            {
                TwoColumnHelpRow[] commandArguments = ctx.HelpBuilder.GetCommandArgumentRows(ctx.Command, ctx).ToArray();

                if (commandArguments.Length <= 0)
                {
                    ctx.WasSectionSkipped = true;
                    return;
                }

                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpArgumentsTitle(), null, ctx.Output);
                ctx.HelpBuilder.WriteColumns(commandArguments, ctx);
            };

        ///  <summary>
        /// Writes a help section describing a command's subcommands.
        ///  </summary>
        public static HelpSectionDelegate SubcommandsSection() =>
            ctx => ctx.HelpBuilder.WriteSubcommands(ctx);

        ///  <summary>
        /// Writes a help section describing a command's options.
        ///  </summary>
        public static HelpSectionDelegate OptionsSection() =>
            ctx =>
            {
                List<TwoColumnHelpRow> options = new();
                HashSet<IOption> uniqueOptions = new(); // global help aliases may be duplicated, we just ignore them
                foreach (Option option in ctx.Command.Options)
                {
                    if (!option.IsHidden && uniqueOptions.Add(option))
                    {
                        options.Add(ctx.HelpBuilder.GetTwoColumnRow(option, ctx));
                    }
                }

                Command? current = ctx.Command as Command;
                while (current is not null)
                {
                    Command? parentCommand = null;
                    for (int parentIndex = 0; parentIndex < current.Parents.Count; parentIndex++)
                    {
                        if ((parentCommand = current.Parents[parentIndex] as Command) is not null)
                        {
                            foreach (Option option in parentCommand.Options)
                            {
                                if (option.IsGlobal && !option.IsHidden && uniqueOptions.Add(option))
                                {
                                    options.Add(ctx.HelpBuilder.GetTwoColumnRow(option, ctx));
                                }
                            }

                            break;
                        }
                    }
                    current = parentCommand;
                }

                if (options.Count <= 0)
                {
                    ctx.WasSectionSkipped = true;
                    return;
                }

                ctx.HelpBuilder.WriteHeading(ctx.HelpBuilder.LocalizationResources.HelpOptionsTitle(), null, ctx.Output);
                ctx.HelpBuilder.WriteColumns(options, ctx);
                ctx.Output.WriteLine();
            };

        ///  <summary>
        /// Writes a help section describing a command's additional arguments, typically shown only when <see cref="Command.TreatUnmatchedTokensAsErrors"/> is set to <see langword="true"/>.
        ///  </summary>
        public static HelpSectionDelegate AdditionalArgumentsSection() =>
            ctx => ctx.HelpBuilder.WriteAdditionalArguments(ctx);
    }
}