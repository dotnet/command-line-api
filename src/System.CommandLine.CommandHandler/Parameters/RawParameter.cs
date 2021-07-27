using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{
    public class RawParameter : PropertyParameter
    {
        public RawParameter(string localName, ITypeSymbol valueType) 
            : base(localName, valueType, valueType)
        {
        }

        public override string GetValueFromContext()
            => LocalName;
    }
}
