using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal abstract class Parameter : IEquatable<Parameter>
    {
        protected static SymbolEqualityComparer SymbolComparer { get; } = SymbolEqualityComparer.Default;

        public ITypeSymbol ValueType { get; }

        protected Parameter(ITypeSymbol valueType)
        {
            ValueType = valueType;
        }

        public abstract string GetValueFromContext();

        public virtual string GetPropertyDeclaration() => "";
        public virtual string GetPropertyAssignment() => "";
        public virtual (string Type, string Name) GetMethodParameter() => ("", "");

        public override int GetHashCode()
        {
            return SymbolComparer.GetHashCode(ValueType);
        }

        protected static int HashCode<T>([DisallowNull] T value)
                => EqualityComparer<T>.Default.GetHashCode(value);

        public override bool Equals(object? obj)
        {
            return base.Equals(obj as Parameter);
        }

        public bool Equals(Parameter? other)
        {
            if (other is null) return false;
            return SymbolComparer.Equals(ValueType, other.ValueType);
        }

        protected static bool Equals<T>(T first, T second)
            => EqualityComparer<T>.Default.Equals(first, second);
    }
}
