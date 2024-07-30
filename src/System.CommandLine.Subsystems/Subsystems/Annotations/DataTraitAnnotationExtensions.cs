using System.CommandLine.Validation.Traits;

namespace System.CommandLine.Subsystems.Annotations
{
    public static class TraitAnnotationExtensions
    {
        public static void SetDataTrait<TSymbol, TDataTrait>(this TSymbol symbol, TDataTrait dataTrait)
            where TSymbol : CliSymbol
            where TDataTrait : Trait
        {
            if (!symbol.TryGetAnnotation(GeneralAnnotations.DataTraits, out var dataTraits))
            {
                dataTraits = [];
                symbol.SetAnnotation(GeneralAnnotations.DataTraits, dataTraits);
            }
            dataTraits.Add(dataTrait);
        }

        public static List<Trait>? GetDataTraits(this CliDataSymbol symbol) 
        {
            return symbol.GetAnnotationOrDefault(GeneralAnnotations.DataTraits);
        }

        public static List<Trait>? GetCommandTraits(this CliCommand symbol)
        {
            return symbol.GetAnnotationOrDefault(GeneralAnnotations.DataTraits);
        }
    }
}
