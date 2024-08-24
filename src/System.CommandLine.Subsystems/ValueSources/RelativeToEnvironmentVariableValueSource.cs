// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public class RelativeToEnvironmentVariableValueSource<T>(string environmentVariableName,
                                                         Func<string?, (bool success, T? value)>? calculation = null,
                                                         string? description = null)
    : ValueSource<T>
{
    public override string? Description { get; } = description;

    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        string? stringValue = Environment.GetEnvironmentVariable(environmentVariableName);

        if (stringValue is null)
        {
            value = default;
            return false;
        }

        // TODO: Unify this with System.CommandLine.ArgumentConverter conversions, which will require changes to that code.
        //       This will provide consistency, including support for nullable value types, and custom type conversions
        try
        {
            if (calculation is not null)
            {
                (var success, var calcValue) = calculation(stringValue);
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

