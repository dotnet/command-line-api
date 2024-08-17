using System.CommandLine;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine
{
    public static class ValueConditionAnnotationExtensions
    {
        public static void SetRange<T>(this CliValueSymbol symbol, T lowerBound, T upperBound)
            where T : IComparable<T>
        {
            var range = new Range
            {
                ValueType = symbol.ValueType,
                LowerBound = lowerBound,
                UpperBound = upperBound
            };

            symbol.SetValueCondition(range);
        }

        public static void SetInclusiveGroup(this CliCommand symbol, IEnumerable<CliValueSymbol> group)
            => symbol.SetValueCondition(new InclusiveGroup(group));

        public static void SetValueCondition<TSymbol, TValueCondition>(this TSymbol symbol, TValueCondition valueCondition)
            where TSymbol : CliSymbol
            where TValueCondition : ValueCondition
        {
            if (!symbol.TryGetAnnotation<List<ValueCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions))
            {
                valueConditions = [];
                symbol.SetAnnotation(ValueConditionAnnotations.ValueConditions, valueConditions);
            }
            valueConditions.Add(valueCondition);
        }

        public static List<ValueCondition>? GetValueConditions(this CliSymbol symbol) 
            => symbol.TryGetAnnotation<List<ValueCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions)
                ? valueConditions
                : null;

    }
}
