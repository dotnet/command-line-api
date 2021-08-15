using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.CommandLine.CommandGenerator.Parameters;

namespace System.CommandLine.CommandGenerator
{
    internal class WellKnownTypes
    {
        public INamedTypeSymbol Console { get; }
        public INamedTypeSymbol ParseResult { get; }
        public INamedTypeSymbol InvocationContext { get; }
        public INamedTypeSymbol HelpBuilder { get; }
        public INamedTypeSymbol BindingContext { get; }
        public IEqualityComparer<Microsoft.CodeAnalysis.ISymbol?> Comparer { get; }

        public WellKnownTypes(Compilation compilation, IEqualityComparer<Microsoft.CodeAnalysis.ISymbol?> comparer)
        {
            Console = GetType("System.CommandLine.IConsole");
            ParseResult = GetType("System.CommandLine.Parsing.ParseResult");
            InvocationContext = GetType("System.CommandLine.Invocation.InvocationContext");
            HelpBuilder = GetType("System.CommandLine.Help.IHelpBuilder");
            BindingContext = GetType("System.CommandLine.Binding.BindingContext");

            INamedTypeSymbol GetType(string typeName)
                => compilation.GetTypeByMetadataName(typeName)
                ?? throw new InvalidOperationException($"Could not find well known type '{typeName}'");
            Comparer = comparer;
        }

        internal bool Contains(Microsoft.CodeAnalysis.ISymbol symbol) => TryGet(symbol, out _);

        internal bool TryGet(Microsoft.CodeAnalysis.ISymbol symbol, out Parameter? parameter)
        {
            if (Comparer.Equals(Console, symbol))
            {
                parameter = new ConsoleParameter(Console);
                return true;
            }
            if (Comparer.Equals(InvocationContext, symbol))
            {
                parameter = new InvocationContextParameter(InvocationContext);
                return true;
            }
            if (Comparer.Equals(ParseResult, symbol))
            {
                parameter = new ParseResultParameter(ParseResult);
                return true;
            }
            if (Comparer.Equals(HelpBuilder, symbol))
            {
                parameter = new HelpBuilderParameter(HelpBuilder);
                return true;
            }
            if (Comparer.Equals(BindingContext, symbol))
            {
                parameter = new BindingContextParameter(BindingContext);
                return true;
            }
            parameter = null;
            return false;
        }
    }
}
