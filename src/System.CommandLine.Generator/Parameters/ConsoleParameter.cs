using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal class ConsoleParameter : Parameter, IEquatable<ConsoleParameter>
    {
        public ConsoleParameter(ITypeSymbol consoleType)
            : base(consoleType)
        {
        }

        public override string GetValueFromContext()
            => "context.Console";

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as ConsoleParameter);

        public bool Equals(ConsoleParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
