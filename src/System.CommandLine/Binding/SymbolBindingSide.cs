// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public class SymbolBindingSide : BindingSide
    {
        private SymbolBindingSide(Option option)
            : base(GetOptionRetrieve(option), GetOptionAssign(option))
            => Symbol = option;

        public SymbolBindingSide(Argument argument)
            : base(GetArgumentRetrieve(argument), GetArgumentAssign(argument)) 
            => Symbol = argument;

        public static SymbolBindingSide Create(Option symbol)
            => new SymbolBindingSide(symbol);

        public static SymbolBindingSide Create(Argument argument)
            => new SymbolBindingSide(argument);

        public ISymbolBase Symbol { get; }

        private static BindingGetter GetOptionRetrieve(Option option)
            => (context, target) => context.ParseResult.GetValueOrDefault(option, true);

        private static BindingSetter GetOptionAssign(Option option)
            => (context, target, value) => option.Argument.SetDefaultValue(value);

        private static BindingGetter GetArgumentRetrieve(Argument argument)
            => (context, target) => context.ParseResult.GetValueOrDefault(argument, true);

        private static BindingSetter GetArgumentAssign(Argument argument)
            => (context, target, value) => argument.SetDefaultValue(value);
    }
}
