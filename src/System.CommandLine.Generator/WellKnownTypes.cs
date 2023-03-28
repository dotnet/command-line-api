// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Generator.Parameters;
using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator
{
    internal class WellKnownTypes
    {
        public INamedTypeSymbol ParseResult { get; }
        public IEqualityComparer<ISymbol?> Comparer { get; }

        public WellKnownTypes(Compilation compilation, IEqualityComparer<ISymbol?> comparer)
        {
            ParseResult = GetType("System.CommandLine.ParseResult");

            INamedTypeSymbol GetType(string typeName)
                => compilation.GetTypeByMetadataName(typeName)
                   ?? throw new InvalidOperationException($"Could not find well known type '{typeName}'");

            Comparer = comparer;
        }

        internal bool Contains(ISymbol symbol) => TryGet(symbol, out _);

        internal bool TryGet(ISymbol symbol, out Parameter? parameter)
        {
            if (Comparer.Equals(ParseResult, symbol))
            {
                parameter = new ParseResultParameter(ParseResult);
                return true;
            }

            if (symbol.MetadataName == "System.CommandLine.Binding.BindingContext" && symbol is INamedTypeSymbol bindingContext)
            {
                parameter = new BindingContextParameter(bindingContext);
                return true;
            }

            parameter = null;
            return false;
        }
    }
}