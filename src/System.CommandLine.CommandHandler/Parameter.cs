using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler
{
    public abstract class Parameter
    {
        public ITypeSymbol ValueType { get; }

        protected Parameter(ITypeSymbol valueType)
        {
            ValueType = valueType;
        }

        public abstract string GetValueFromContext();

        public virtual string GetPropertyDeclaration() => "";
        public virtual string GetPropertyAssignment() => "";
        public virtual (string Type, string Name) GetMethodParameter() => ("", "");
    }
}
