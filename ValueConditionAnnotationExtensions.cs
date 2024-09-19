using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.Validation;
using System.CommandLine.ValueConditions;
using System.CommandLine.ValueSources;

namespace System.CommandLine;

/// <summary>
/// Contains the extension methods that are used to create value conditions
/// </summary>
public static class ValueConditionAnnotationExtensions
{
    /// <summary>
    /// Set the upper and/or lower bound values of the range.
    /// </summary>
    /// <typeparam name="T">The type of the bounds.</typeparam>
    /// <param name="symbol">The option or argument the range applies to.</param>
    /// <param name="lowerBound">The lower bound of the range.</param>
    /// <param name="upperBound">The upper bound of the range.</param>
    // TODO: Add RangeBounds
    // TODO: You should not have to set both...why not nullable?
    public static void SetRange<T>(this CliValueSymbol symbol, T lowerBound, T upperBound)
        where T : IComparable<T>
    {
        var range = new Range<T>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    /// <summary>
    /// Set the possible casing value for the string input value.
    /// </summary>
    /// <typeparam name="T">The type of the bounds.</typeparam>
    /// <param name="symbol">The option or argument the range applies to.</param>
    /// <param name="casing">This could either be lower or upper.</param>
    public static void SetCasing(this CliValueSymbol symbol, string casing)
    {
        symbol.SetValueCondition(new StringCase(casing));
    }

    /// <summary>
    /// Set the upper and/or lower bound via ValueSource. Implicit conversions means this 
    /// generally just works with any <see cref="ValueSource">.
    /// </summary>
    /// <typeparam name="T">The type of the bounds.</typeparam>
    /// <param name="symbol">The option or argument the range applies to.</param>
    /// <param name="lowerBound">The <see cref="ValueSource"> that is the lower bound of the range.</param>
    /// <param name="upperBound">The <see cref="ValueSource"> that is the upper bound of the range.</param>
    // TODO: Add RangeBounds
    // TODO: You should not have to set both...why not nullable?
    public static void SetRange<T>(this CliValueSymbol symbol, ValueSource<T> lowerBound, ValueSource<T> upperBound)
        where T : IComparable<T>
        // TODO: You should not have to set both...why not nullable?
    {
        var range = new Range<T>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    /// <summary>
    /// Indicates that there is an inclusive group of options and arguments for the command. All
    /// members of an inclusive must be present, or none can be present.
    /// </summary>
    /// <param name="command">The command the inclusive group applies to.</param>
    /// <param name="group">The group of options and arguments that must all be present, or none can be present.</param>
    public static void SetInclusiveGroup(this CliCommand command, IEnumerable<CliValueSymbol> group)
        => command.SetValueCondition(new InclusiveGroup(group));

    // TODO: This should not be public if ValueConditions are not public
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

    // TODO: This should not be public if ValueConditions are not public
    public static void SetValueCondition<TCommandCondition>(this CliCommand symbol, TCommandCondition commandCondition)
        where TCommandCondition : CommandCondition
    {
        if (!symbol.TryGetAnnotation<List<CommandCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions))
        {
            valueConditions = [];
            symbol.SetAnnotation(ValueConditionAnnotations.ValueConditions, valueConditions);
        }
        valueConditions.Add(commandCondition);
    }

    /// <summary>
    /// Gets a list of conditions on an option or argument.
    /// </summary>
    /// <param name="command">The option or argument to get the conditions for.</param>
    /// <returns>The conditions that have been applied to the option or argument.</returns>
    /// 
    // TODO: This is public because it will be used by other subsystems we might not own. It could be an extension method the subsystem namespace
    public static List<ValueCondition>? GetValueConditions(this CliValueSymbol symbol)
        => symbol.TryGetAnnotation<List<ValueCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions)
            ? valueConditions
            : null;

    /// <summary>
    /// Gets a list of conditions on a command.
    /// </summary>
    /// <param name="command">The command to get the conditions for.</param>
    /// <returns>The conditions that have been applied to the command.</returns>
    /// 
    // TODO: This is public because it will be used by other subsystems we might not own. It could be an extension method the subsystem namespace
    public static List<CommandCondition>? GetCommandConditions(this CliCommand command)
        => command.TryGetAnnotation<List<CommandCondition>>(ValueConditionAnnotations.ValueConditions, out var valueConditions)
            ? valueConditions
            : null;

    /// <summary>
    /// Gets the condition that matches the type, if it exists on this option or argument.
    /// </summary>
    /// <typeparam name="TCondition">The type of condition to return.</typeparam>
    /// <param name="symbol">The option or argument that may contain the condition.</param>
    /// <returns>The condition if it exists on the option or argument, otherwise null.</returns>
    // This method feels useful because it clarifies that last should win and returns one, when only one should be applied
    // TODO: Consider removing user facing naming, other than the base type, that is Value or CommandCondition and just use Condition
    public static TCondition? GetValueCondition<TCondition>(this CliValueSymbol symbol)
        where TCondition : ValueCondition
        => !symbol.TryGetAnnotation(ValueConditionAnnotations.ValueConditions, out List<ValueCondition>? valueConditions)
            ? null
            : valueConditions.OfType<TCondition>().LastOrDefault();

    /// <summary>
    /// Gets the condition that matches the type, if it exists on this command.
    /// </summary>
    /// <typeparam name="TCondition">The type of condition to return.</typeparam>
    /// <param name="symbol">The command that may contain the condition.</param>
    /// <returns>The condition if it exists on the command, otherwise null.</returns>
    // This method feels useful because it clarifies that last should win and returns one, when only one should be applied
    public static TCondition? GetCommandCondition<TCondition>(this CliCommand symbol)
        where TCondition : CommandCondition
        => !symbol.TryGetAnnotation(ValueConditionAnnotations.ValueConditions, out List<CommandCondition>? valueConditions)
            ? null
            : valueConditions.OfType<TCondition>().LastOrDefault();


}
