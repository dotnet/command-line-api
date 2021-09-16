using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal class HelpBuilderParameter : Parameter, IEquatable<HelpBuilderParameter>
    {
        public HelpBuilderParameter(ITypeSymbol helpBuilderType)
            : base(helpBuilderType)
        {
        }

        public override string GetValueFromContext()
            => "context.HelpBuilder";

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as HelpBuilderParameter);

        public bool Equals(HelpBuilderParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
