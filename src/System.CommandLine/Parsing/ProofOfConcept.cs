// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    public static class ProofOfConcept
    {
        public static (T1?, T2?) Parse<T1, T2>(string args, ICliSymbol<T1> symbol1, ICliSymbol<T2> symbol2)
        {
            CliRootCommand command = new();

            Add(symbol1, command);
            Add(symbol2, command);

            ParseResult parseResult = command.Parse(args);

            return (GetValue(parseResult, symbol1), GetValue(parseResult, symbol2));
        }

        public static (T1?, T2?, T3?) Parse<T1, T2, T3>(string args, ICliSymbol<T1> symbol1, ICliSymbol<T2> symbol2, ICliSymbol<T3> symbol3)
        {
            CliRootCommand command = new();

            Add(symbol1, command);
            Add(symbol2, command);
            Add(symbol3, command);

            ParseResult parseResult = command.Parse(args);

            return (GetValue(parseResult, symbol1), GetValue(parseResult, symbol2), GetValue(parseResult, symbol3));
        }

        public static (T1?, T2?, T3?, T4?) Parse<T1, T2, T3, T4>(string args, ICliSymbol<T1> symbol1, ICliSymbol<T2> symbol2, ICliSymbol<T3> symbol3, ICliSymbol<T4> symbol4)
        {
            CliRootCommand command = new();

            Add(symbol1, command);
            Add(symbol2, command);
            Add(symbol3, command);
            Add(symbol4, command);

            ParseResult parseResult = command.Parse(args);

            return (GetValue(parseResult, symbol1), GetValue(parseResult, symbol2), GetValue(parseResult, symbol3), GetValue(parseResult, symbol4));
        }

        public static (T1?, T2?, T3?, T4?, T5?) Parse<T1, T2, T3, T4, T5>(string args, ICliSymbol<T1> symbol1, ICliSymbol<T2> symbol2, ICliSymbol<T3> symbol3, ICliSymbol<T4> symbol4, ICliSymbol<T5> symbol5)
        {
            CliRootCommand command = new();

            Add(symbol1, command);
            Add(symbol2, command);
            Add(symbol3, command);
            Add(symbol4, command);
            Add(symbol5, command);

            ParseResult parseResult = command.Parse(args);

            return (GetValue(parseResult, symbol1), GetValue(parseResult, symbol2), GetValue(parseResult, symbol3), GetValue(parseResult, symbol4), GetValue(parseResult, symbol5));
        }

        private static void Add<T>(ICliSymbol<T> symbol, CliCommand command)
        {
            if (symbol is CliOption<T> option)
            {
                command.Options.Add(option);
            }
            else if (symbol is CliArgument<T> argument)
            {
                command.Arguments.Add(argument);
            }
        }

        private static T? GetValue<T>(ParseResult parseResult, ICliSymbol<T> symbol)
        {
            if (symbol is CliOption<T> option)
            {
                return parseResult.GetValue<T>(option);
            }
            else if (symbol is CliArgument<T> argument)
            {
                return parseResult.GetValue<T>(argument);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

    }
}
