using System.CommandLine;
using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.ValueConditions;

namespace System.CommandLine;

public static class ValueConditionAnnotationExtensions
{
    public static void SetRange<T>(this CliValueSymbol symbol, T lowerBound, T upperBound)
        where T : IComparable<T>
    {
        var range = new Range<T>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    public static void SetRange<T>(this CliValueSymbol symbol, RangeBound<T> lowerBound, T upperBound)
    where T : IComparable<T>
    {
        var range = new Range<T>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    public static void SetRange<T>(this CliValueSymbol symbol, T lowerBound, RangeBound<T> upperBound)
    where T : IComparable<T>
    {
        var range = new Range<T>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    public static void SetRange<T>(this CliValueSymbol symbol, RangeBound<T> lowerBound, RangeBound<T> upperBound)
    where T : IComparable<T>
    {
        var range = new Range<T>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    public static void SetInclusiveGroup(this CliCommand symbol, IEnumerable<CliValueSymbol> group)
        => symbol.SetValueCondition(new InclusiveGroup(group));

    public static void SetValueCondition<TValueSymbol, TValueCondition>(this TValueSymbol symbol, TValueCondition valueCondition)
        where TValueSymbol : CliValueSymbol
        where TValueCondition : ValueCondition
    {
        if (!symbol.TryGetAnnotation<List<ValueCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions))
        {
            valueConditions = [];
            symbol.SetAnnotation(ValueConditionAnnotations.ValueConditions, valueConditions);
        }
        valueConditions.Add(valueCondition);
    }

    public static void SetValueCondition<TValueCondition>(this CliCommand symbol, TValueCondition valueCondition)
        where TValueCondition : CommandCondition
    {
        if (!symbol.TryGetAnnotation<List<CommandCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions))
        {
            valueConditions = [];
            symbol.SetAnnotation(ValueConditionAnnotations.ValueConditions, valueConditions);
        }
        valueConditions.Add(valueCondition);
    }

    public static List<ValueCondition>? GetValueConditions(this CliValueSymbol symbol)
        => symbol.TryGetAnnotation<List<ValueCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions)
            ? valueConditions
            : null;

    public static List<CommandCondition>? GetCommandConditions(this CliCommand symbol)
        => symbol.TryGetAnnotation<List<CommandCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions)
            ? valueConditions
            : null;

    public static TCondition? GetValueCondition<TCondition>(this CliValueSymbol symbol)
        where TCondition : ValueCondition
        => !symbol.TryGetAnnotation(ValueConditionAnnotations.ValueConditions, out List<ValueCondition>? valueConditions)
            ? null
            : valueConditions.OfType<TCondition>().LastOrDefault();

    public static TCondition? GetCommandCondition<TCondition>(this CliCommand symbol)
        where TCondition : CommandCondition
        => !symbol.TryGetAnnotation(ValueConditionAnnotations.ValueConditions, out List<CommandCondition>? valueConditions)
            ? null
            : valueConditions.OfType<TCondition>().LastOrDefault();


}
