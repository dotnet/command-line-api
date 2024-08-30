// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

/// <summary>
/// <see cref="ValueSource"/> that returns the converted value of the specified environment variable.
/// If the calculation delegate is supplied, the returned value of the calculation is returned.
/// </summary>
/// <typeparam name="T">The type to be returned, which is almost always the type of the symbol the ValueSource will be used for.</typeparam>
/// <param name="environmentVariableName">The name of then environment variable. Note that for some systems, this is case sensitive.</param>
/// <param name="calculation">A delegate that returns the requested type. If it is not specified, standard type conversions are used.</param>
/// <param name="description">The description of this value, used to clarify the intent of the values that appear in error messages.</param>
public sealed class RelativeToEnvironmentVariableValueSource<T>
    : ValueSource<T>
{
    internal RelativeToEnvironmentVariableValueSource(
        string environmentVariableName,
        Func<string?, (bool success, T? value)>? calculation = null,
        string? description = null)
    {
        EnvironmentVariableName = environmentVariableName;
        Calculation = calculation;
        Description = description;
    }

    public string EnvironmentVariableName { get; }
    public Func<string?, (bool success, T? value)>? Calculation { get; }

    /// <summary>
    /// The description of this value, used to clarify the intent of the values that appear in error messages.
    /// </summary>
    public override string? Description { get; } 

    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        string? stringValue = Environment.GetEnvironmentVariable(EnvironmentVariableName);

        if (stringValue is null)
        {
            value = default;
            return false;
        }

        // TODO: Unify this with System.CommandLine.ArgumentConverter conversions, which will require changes to that code.
        //       This will provide consistency, including support for nullable value types, and custom type conversions
        try
        {
            if (Calculation is not null)
            {
                (var success, var calcValue) = Calculation(stringValue);
                if (success)
                {
                    value = calcValue;
                    return true;
                }
                value = default;
                return false;
            }
            var newValue = Convert.ChangeType(stringValue, typeof(T));
            value = (T?)newValue;
            return true;
        }
        catch
        {
            // TODO: This probably represents a failure converting from string, so in user's world to fix. How do we report this?
            value = default;
            return false;
        }
    }
}

