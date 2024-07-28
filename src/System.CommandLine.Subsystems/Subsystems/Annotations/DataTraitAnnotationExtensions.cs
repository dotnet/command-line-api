using System.CommandLine.Validation.DataTraits;

namespace System.CommandLine.Subsystems.Annotations
{
    public static class DataTraitAnnotationExtensions
    {
        public static void SetDataTrait<TSymbol, TDataTrait>(this TSymbol symbol, TDataTrait dataTrait)
            where TSymbol : CliSymbol
            where TDataTrait : DataTrait
        {
            if (!symbol.TryGetAnnotation(GeneralAnnotations.DataTraits, out var dataTraits))
            {
                dataTraits = [];
                symbol.SetAnnotation(GeneralAnnotations.DataTraits, dataTraits);
            }
            dataTraits.Add(dataTrait);
        }

        public static List<DataTrait>? GetDataTraits<TSymbol>(this TSymbol symbol) 
            where TSymbol : CliSymbol
        {
            return symbol.GetAnnotationOrDefault(GeneralAnnotations.DataTraits);
        }
    }
}
