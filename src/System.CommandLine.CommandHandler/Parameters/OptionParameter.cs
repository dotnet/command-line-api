﻿using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{

    public class OptionParameter : PropertyParameter, IEquatable<OptionParameter>
    {
        public OptionParameter(string localName, INamedTypeSymbol type, ITypeSymbol valueType)
            : base(localName, type, valueType)
        {
        }

        public override string GetValueFromContext()
            => $"context.ParseResult.ValueForOption({LocalName})";

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object obj)
            => Equals(obj as OptionParameter);

        public bool Equals(OptionParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
