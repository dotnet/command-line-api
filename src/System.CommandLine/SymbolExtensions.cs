// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    internal static class SymbolExtensions
    {
        internal static IEnumerable<string> ChildSymbolAliases(this ISymbol symbol) =>
            symbol.Children
                .Where(s => !s.IsHidden)
                .SelectMany(s => s.RawAliases);

        internal static bool ShouldShowHelp(this ISymbol symbol) =>
            !symbol.IsHidden &&
            (!String.IsNullOrWhiteSpace(symbol.Name) ||
             !String.IsNullOrWhiteSpace(symbol.Description) ||
             symbol.Arguments().Any(a => a.ShouldShowHelp()));

        internal static bool ShouldShowHelp(
            this IArgument argument) =>
            argument != null &&
            !String.IsNullOrWhiteSpace(argument.Name) &&
            argument.Arity.MaximumNumberOfValues > 0;

        internal static IReadOnlyCollection<IArgument> Arguments(this ISymbol symbol)
        {
            switch (symbol)
            {
                case IOption option:
                    return new[]
                    {
                        option.Argument
                    };
                case ICommand command:
                    return command.Arguments;
                case IArgument argument:
                    return new[]
                    {
                        argument
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        internal static OptionResult CreateImplicitResult(
            this IOption option,
            CommandResult parent)
        {
            var result = new OptionResult(option, 
                                          option.CreateImplicitToken());

            if (option.Argument.HasDefaultValue)
            {
                var value = option.Argument.GetDefaultValue();

                switch (value)
                {
                    case string arg:
                        result.TryTakeToken(
                            new Token(arg, TokenType.Argument));
                        break;

                    default:
                        result.ArgumentResults.Add(
                            ArgumentResult.Success(option.Argument, value));
                        break;
                }
            }

            return result;
        }

        internal static Token CreateImplicitToken(this IOption option)
        {
            var optionName = option.Name;

            var defaultAlias = option.RawAliases.First(alias => alias.RemovePrefix() == optionName);

            return new ImplicitToken(defaultAlias, TokenType.Option);
        }
    }
}
