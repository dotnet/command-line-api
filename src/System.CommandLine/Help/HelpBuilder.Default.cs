// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Linq;

namespace System.CommandLine.Help;

internal partial class HelpBuilder
{
    /// <summary>
    /// Provides default formatting for help output.
    /// </summary>
    public static class Default
    {
        /// <summary>
        /// Gets an argument's default value to be displayed in help.
        /// </summary>
        /// <param name="symbol">The argument or option to get the default value for.</param>
        public static string GetArgumentDefaultValue(Symbol symbol)
        {
            return symbol switch
            {
                Argument argument => ShouldShowDefaultValue(argument) 
                                         ? ToString(argument.GetDefaultValue()) 
                                         : "",
                Option option => ShouldShowDefaultValue(option) 
                                     ? ToString(option.GetDefaultValue()) 
                                     : "",
                _ => throw new InvalidOperationException("Symbol must be an Argument or Option.")
            };

            static string ToString(object? value) => value switch
            {
                null => string.Empty,
                string str => str,
                IEnumerable enumerable => string.Join("|", enumerable.Cast<object>()),
                _ => value.ToString() ?? string.Empty
            };
        }

        public static bool ShouldShowDefaultValue(Symbol symbol) =>
            symbol switch
            {
                Option option => ShouldShowDefaultValue(option),
                Argument argument => ShouldShowDefaultValue(argument),
                _ => false
            };

        public static bool ShouldShowDefaultValue(Option option) =>
            option.HasDefaultValue && 
            !(option.ValueType == typeof(bool) || option.ValueType == typeof(bool?));

        public static bool ShouldShowDefaultValue(Argument argument) =>
            argument.HasDefaultValue && 
            !(argument.ValueType == typeof(bool) || argument.ValueType == typeof(bool?));

        /// <summary>
        /// Gets the description for an argument (typically used in the second column text in the arguments section).
        /// </summary>
        public static string GetArgumentDescription(Argument argument) => argument.Description ?? string.Empty;

        /// <summary>
        /// Gets the usage title for an argument (for example: <c>&lt;value&gt;</c>, typically used in the first column text in the arguments usage section, or within the synopsis.
        /// </summary>
        public static string GetArgumentUsageLabel(Symbol parameter)
        {
            // By default Option.Name == Argument.Name, don't repeat it
            return parameter switch
            {
                Argument argument => GetUsageLabel(argument.HelpName, argument.ValueType, argument.CompletionSources, argument, argument.Arity) ?? $"<{argument.Name}>",
                Option option => GetUsageLabel(option.HelpName, option.ValueType, option.CompletionSources, option, option.Arity) ?? "",
                _ => throw new InvalidOperationException()
            };

            static string? GetUsageLabel(
                string? helpName,
                Type valueType,
                List<Func<CompletionContext, IEnumerable<CompletionItem>>> completionSources,
                Symbol symbol,
                ArgumentArity arity)
            {
                if (!string.IsNullOrWhiteSpace(helpName))
                {
                    return $"<{helpName}>";
                }

                if (valueType == typeof(bool) ||
                    valueType == typeof(bool?) ||
                    arity.MaximumNumberOfValues <= 0) // allowing zero arguments means we don't need to show usage
                {
                    return null;
                }

                if (completionSources.Count <= 0)
                {
                    if (symbol is Option)
                    {
                        return $"<{symbol.Name.TrimStart('-', '/')}>";
                    }

                    return null;
                }

                IEnumerable<string> completions = symbol
                                                  .GetCompletions(CompletionContext.Empty)
                                                  .Select(item => item.Label);

                string joined = string.Join("|", completions);

                if (!string.IsNullOrEmpty(joined))
                {
                    return $"<{joined}>";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the usage label for the specified symbol (typically used as the first column text in help output).
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <returns>Text to display.</returns>
        public static string GetCommandUsageLabel(Command symbol)
            => GetIdentifierSymbolUsageLabel(symbol, symbol.Aliases);

        /// <inheritdoc cref="GetCommandUsageLabel(Command)"/>
        public static string GetOptionUsageLabel(Option symbol)
            => GetIdentifierSymbolUsageLabel(symbol, symbol.Aliases);

        private static string GetIdentifierSymbolUsageLabel(Symbol symbol, ICollection<string>? aliasSet)
        {
            var aliases = aliasSet is null
                ? new[] { symbol.Name }
                : new[] { symbol.Name }.Concat(aliasSet)
                                .Select(r => r.SplitPrefix())
                                .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                                .GroupBy(t => t.Alias)
                                .Select(t => t.First())
                                .Select(t => $"{t.Prefix}{t.Alias}");

            var firstColumnText = string.Join(", ", aliases);

            foreach (var argument in symbol.GetParameters())
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

            if (symbol is Option { Required: true })
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
                foreach (Option option in ctx.Command.Options.OrderBy(o => o is HelpOption or VersionOption))
                {
                    if (!option.Hidden)
                    {
                        if (option is HelpOption)
                        {
                            addedHelpOption = true;
                        }

                        optionRows.Add(ctx.HelpBuilder.GetTwoColumnRow(option, ctx));
                    }
                }

                Command? current = ctx.Command;
                while (current is not null)
                {
                    Command? parentCommand = null;
                    foreach (Symbol parent in current.Parents)
                    {
                        if ((parentCommand = parent as Command) is not null)
                        {
                            if (parentCommand.Options.Any())
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
        /// Writes a help section describing a command's additional arguments, typically shown only when <see cref="Command.TreatUnmatchedTokensAsErrors"/> is set to <see langword="true"/>.
        ///  </summary>
        public static Func<HelpContext, bool> AdditionalArgumentsSection() =>
            ctx => ctx.HelpBuilder.WriteAdditionalArguments(ctx);
    }
}