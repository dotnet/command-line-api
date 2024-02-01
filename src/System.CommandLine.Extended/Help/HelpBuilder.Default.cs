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
        public static string GetArgumentDefaultValue(CliArgument argument)
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
                        return defaultValue.ToString() ?? "";
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the description for an argument (typically used in the second column text in the arguments section).
        /// </summary>
        public static string GetArgumentDescription(CliArgument argument) => argument.Description ?? string.Empty;

        /// <summary>
        /// Gets the usage title for an argument (for example: <c>&lt;value&gt;</c>, typically used in the first column text in the arguments usage section, or within the synopsis.
        /// </summary>
        public static string GetArgumentUsageLabel(CliArgument argument)
        {
            // Argument.HelpName is always first choice
            if (!string.IsNullOrWhiteSpace(argument.HelpName))
            {
                return $"<{argument.HelpName}>";
            }
            else if (!argument.IsBoolean() && argument.CompletionSources.Count > 0)
            {
                IEnumerable<string> completions = argument
                    .GetCompletions(CompletionContext.Empty)
                    .Select(item => item.Label);

                string joined = string.Join("|", completions);

                if (!string.IsNullOrEmpty(joined))
                {
                    return $"<{joined}>";
                }
            }

            // By default Option.Name == Argument.Name, don't repeat it
            return argument.FirstParent?.Symbol is not CliOption ? $"<{argument.Name}>" : "";
        }

        /// <summary>
        /// Gets the usage label for the specified symbol (typically used as the first column text in help output).
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <returns>Text to display.</returns>
        public static string GetCommandUsageLabel(CliCommand symbol)
            => GetIdentifierSymbolUsageLabel(symbol, symbol._aliases);

        /// <inheritdoc cref="GetCommandUsageLabel(CliCommand)"/>
        public static string GetOptionUsageLabel(CliOption symbol)
            => GetIdentifierSymbolUsageLabel(symbol, symbol._aliases);

        private static string GetIdentifierSymbolUsageLabel(CliSymbol symbol, AliasSet? aliasSet)
        {
            var aliases =  aliasSet is null
                ? new [] { symbol.Name }
                : new [] {symbol.Name}.Concat(aliasSet)
                                .Select(r => r.SplitPrefix())
                                .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                                .GroupBy(t => t.Alias)
                                .Select(t => t.First())
                                .Select(t => $"{t.Prefix}{t.Alias}");

            var firstColumnText = string.Join(", ", aliases);

            foreach (var argument in symbol.Arguments())
            {
                if (!argument.Hidden)
                {
                    var argumentFirstColumnText = GetArgumentUsageLabel(argument);

                    if (!string.IsNullOrWhiteSpace(argumentFirstColumnText))
                    {
                        firstColumnText += $" {argumentFirstColumnText}";
                    }
                }
            }

            if (symbol is CliOption { Required: true })
            {
                firstColumnText += $" {LocalizationResources.HelpOptionsRequiredLabel()}";
            }

            return firstColumnText;
        }

        /// <summary>
        /// Gets the default sections to be written for command line help.
        /// </summary>
        public static IEnumerable<Func<HelpContext, bool>> GetLayout()
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
        public static Func<HelpContext, bool> SynopsisSection() =>
            ctx =>
            {
                ctx.HelpBuilder.WriteHeading(LocalizationResources.HelpDescriptionTitle(), ctx.Command.Description, ctx.Output);
                return true;
            };

        /// <summary>
        /// Writes a help section describing a command's usage.
        /// </summary>
        public static Func<HelpContext, bool> CommandUsageSection() =>
            ctx =>
            {
                ctx.HelpBuilder.WriteHeading(LocalizationResources.HelpUsageTitle(), ctx.HelpBuilder.GetUsage(ctx.Command), ctx.Output);
                return true;
            };

        ///  <summary>
        /// Writes a help section describing a command's arguments.
        ///  </summary>
        public static Func<HelpContext, bool> CommandArgumentsSection() =>
            ctx =>
            {
                TwoColumnHelpRow[] commandArguments = ctx.HelpBuilder.GetCommandArgumentRows(ctx.Command, ctx).ToArray();

                if (commandArguments.Length > 0)
                {
                    ctx.HelpBuilder.WriteHeading(LocalizationResources.HelpArgumentsTitle(), null, ctx.Output);
                    ctx.HelpBuilder.WriteColumns(commandArguments, ctx);
                    return true;
                }

                return false;
            };

        ///  <summary>
        /// Writes a help section describing a command's subcommands.
        ///  </summary>
        public static Func<HelpContext, bool> SubcommandsSection() =>
            ctx => ctx.HelpBuilder.WriteSubcommands(ctx);

        ///  <summary>
        /// Writes a help section describing a command's options.
        ///  </summary>
        public static Func<HelpContext, bool> OptionsSection() =>
            ctx =>
            {
                List<TwoColumnHelpRow> optionRows = new();
                bool addedHelpOption = false;

                if (ctx.Command.HasOptions)
                {
                    foreach (CliOption option in ctx.Command.Options)
                    {
                        if (!option.Hidden)
                        {
                            optionRows.Add(ctx.HelpBuilder.GetTwoColumnRow(option, ctx));
                            if (option is HelpOption)
                            {
                                addedHelpOption = true;
                            }
                        }
                    }
                }

                CliCommand? current = ctx.Command;
                while (current is not null)
                {
                    CliCommand? parentCommand = null;
                    SymbolNode? parent = current.FirstParent;
                    while (parent is not null)
                    {
                        if ((parentCommand = parent.Symbol as CliCommand) is not null)
                        {
                            if (parentCommand.HasOptions)
                            {
                                foreach (var option in parentCommand.Options)
                                {
                                    // global help aliases may be duplicated, we just ignore them
                                    if (option is { Recursive: true, Hidden: false })
                                    {
                                        if (option is not HelpOption || !addedHelpOption)
                                        {
                                            optionRows.Add(ctx.HelpBuilder.GetTwoColumnRow(option, ctx));
                                        }
                                    }
                                }
                            }

                            break;
                        }
                        parent = parent.Next;
                    }
                    current = parentCommand;
                }

                if (optionRows.Count > 0)
                {
                    ctx.HelpBuilder.WriteHeading(LocalizationResources.HelpOptionsTitle(), null, ctx.Output);
                    ctx.HelpBuilder.WriteColumns(optionRows, ctx);
                    return true;
                }

                return false;
            };

        ///  <summary>
        /// Writes a help section describing a command's additional arguments, typically shown only when <see cref="CliCommand.TreatUnmatchedTokensAsErrors"/> is set to <see langword="true"/>.
        ///  </summary>
        public static Func<HelpContext, bool> AdditionalArgumentsSection() =>
            ctx => ctx.HelpBuilder.WriteAdditionalArguments(ctx);
    }
}