﻿using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandGenerator.Parameters
{
    internal class ParseResultParameter : Parameter, IEquatable<ParseResultParameter>
    {
        public ParseResultParameter(ITypeSymbol parseResultType)
            : base(parseResultType)
        {
        }

        public override string GetValueFromContext()
            => "context.ParseResult";

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as ParseResultParameter);

        public bool Equals(ParseResultParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
