namespace System.CommandLine
{
    static internal class SymbolDefinitionExtensions
    {
        public static bool IsHidden(this SymbolDefinition symbolDefinition) =>
            String.IsNullOrWhiteSpace(symbolDefinition.Description);
    }
}