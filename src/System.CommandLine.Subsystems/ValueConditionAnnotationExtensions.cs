// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.ValueConditions;
using System.CommandLine.ValueSources;

namespace System.CommandLine;

/// <summary>
/// Contains the extension methods that are used to create value conditions
/// </summary>
public static class ValueConditionAnnotationExtensions
{
    /// <summary>
    /// Set upper and/or lower bounds on the range of values that the symbol value may have.
    /// </summary>
    /// <typeparam name="TValueSymbol">The type of the symbol whose value is bounded by the range.</typeparam>
    /// <typeparam name="TValue">The type of the value that is bounded by the range.</typeparam>
    /// <param name="symbol">The option or argument the range applies to.</param>
    /// <param name="lowerBound">The lower bound of the range.</param>
    /// <param name="upperBound">The upper bound of the range.</param>
    // TODO: can we eliminate this overload and just reply on the implicit cast to ValueSource<TValue>?
    public static void SetRange<TValueSymbol, TValue>(this TValueSymbol symbol, TValue lowerBound, TValue upperBound)
        where TValueSymbol : CliValueSymbol, ICliValueSymbol<TValue>
        where TValue : IComparable<TValue>
    {
        var range = new Range<TValue>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    /// <summary>
    /// Set upper and/or lower <see cref="ValueSource"> bounds on the range of values that the symbol value may have.
    /// Implicit conversions means this generally just works with any <see cref="ValueSource">.
    /// </summary>
    /// <typeparam name="TValueSymbol">The type of the symbol whose value is bounded by the range.</typeparam>
    /// <typeparam name="TValue">The type of the value that is bounded by the range.</typeparam>
    /// <param name="symbol">The option or argument the range applies to.</param>
    /// <param name="lowerBound">The <see cref="ValueSource"> that is the lower bound of the range.</param>
    /// <param name="upperBound">The <see cref="ValueSource"> that is the upper bound of the range.</param>
    public static void SetRange<TValueSymbol, TValue>(this TValueSymbol symbol, ValueSource<TValue>? lowerBound, ValueSource<TValue>? upperBound)
        where TValueSymbol : CliValueSymbol, ICliValueSymbol<TValue>
        where TValue : IComparable<TValue>
    {
        var range = new Range<TValue>(lowerBound, upperBound);

        symbol.SetValueCondition(range);
    }

    /// <summary>
    /// Get the upper and/or lower bound of the symbol's value.
    /// </summary>
    /// <param name="symbol">The option or argument the range applies to.</param>
    public static ValueConditions.Range? GetRange(this CliValueSymbol symbol)
        => symbol.GetValueCondition<ValueConditions.Range>();

    /// <summary>
    /// Indicates that there is an inclusive group of options and arguments for the command. All
    /// members of an inclusive must be present, or none can be present.
    /// </summary>
    /// <param name="command">The command the inclusive group applies to.</param>
    /// <param name="group">The group of options and arguments that must all be present, or none can be present.</param>
    public static void SetInclusiveGroup(this CliCommand command, IEnumerable<CliValueSymbol> group)
        => command.SetValueCondition(new InclusiveGroup(group));

    public static void SetValueCondition<TValueSymbol, TValueCondition>(this TValueSymbol symbol, TValueCondition valueCondition)
        where TValueSymbol : CliValueSymbol
        where TValueCondition : ValueCondition
        => symbol.AddAnnotation(ValueConditionAnnotations.ValueConditions, valueCondition);

    public static void SetValueCondition<TCommandCondition>(this CliCommand symbol, TCommandCondition commandCondition)
        where TCommandCondition : CommandCondition
        => symbol.AddAnnotation(ValueConditionAnnotations.ValueConditions, commandCondition);

    /// <summary>
    /// Gets a list of conditions on an option or argument.
    /// </summary>
    /// <param name="command">The option or argument to get the conditions for.</param>
    /// <returns>The conditions that have been applied to the option or argument.</returns>
    public static IEnumerable<ValueCondition> EnumerateValueConditions(this CliValueSymbol symbol)
        => symbol.EnumerateAnnotations<ValueCondition>(ValueConditionAnnotations.ValueConditions);

    /// <summary>
    /// Gets a list of conditions on an option or argument.
    /// </summary>
    /// <param name="command">The option or argument to get the conditions for.</param>
    /// <returns>The conditions that have been applied to the option or argument.</returns>
    public static IEnumerable<ValueCondition> EnumerateValueConditions(this AnnotationResolver resolver, CliValueSymbol symbol)
        => resolver.Enumerate<ValueCondition>(symbol, ValueConditionAnnotations.ValueConditions);

    /// <summary>
    /// Gets a list of conditions on a command.
    /// </summary>
    /// <param name="command">The command to get the conditions for.</param>
    /// <returns>The conditions that have been applied to the command.</returns>
    public static IEnumerable<CommandCondition> EnumerateCommandConditions(this CliCommand command)
        => command.EnumerateAnnotations<CommandCondition>(ValueConditionAnnotations.ValueConditions);

    /// <summary>
    /// Gets a list of conditions on a command.
    /// </summary>
    /// <param name="command">The command to get the conditions for.</param>
    /// <returns>The conditions that have been applied to the command.</returns>
    ///
    // TODO: This is public because it will be used by other subsystems we might not own. It could be an extension method the subsystem namespace
    public static IEnumerable<CommandCondition> EnumerateCommandConditions(this AnnotationResolver resolver, CliCommand command)
        => resolver.Enumerate<CommandCondition>(command, ValueConditionAnnotations.ValueConditions);

    /// <summary>
    /// Gets the condition that matches the type, if it exists on this option or argument.
    /// </summary>
    /// <typeparam name="TCondition">The type of condition to return.</typeparam>
    /// <param name="symbol">The option or argument that may contain the condition.</param>
    /// <returns>The condition if it exists on the option or argument, otherwise null.</returns>
    public static TCondition? GetValueCondition<TCondition>(this CliValueSymbol symbol)
        where TCondition : ValueCondition
        => symbol.EnumerateValueConditions().OfType<TCondition>().FirstOrDefault();

    /// <summary>
    /// Gets the condition that matches the type, if it exists on this command.
    /// </summary>
    /// <typeparam name="TCondition">The type of condition to return.</typeparam>
    /// <param name="symbol">The command that may contain the condition.</param>
    /// <returns>The condition if it exists on the command, otherwise null.</returns>
    public static TCondition? GetCommandCondition<TCondition>(this CliCommand symbol)
        where TCondition : CommandCondition
        => symbol.EnumerateCommandConditions().OfType<TCondition>().FirstOrDefault();


}
