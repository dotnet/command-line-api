﻿using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{

    public class OptionParameter : PropertyParameter
    {
        public OptionParameter(string localName, INamedTypeSymbol type, ITypeSymbol valueType)
            : base(localName, type, valueType)
        {
        }

        public override string GetValueFromContext()
            => $"context.ParseResult.ValueForOption({LocalName})";
    }
}
