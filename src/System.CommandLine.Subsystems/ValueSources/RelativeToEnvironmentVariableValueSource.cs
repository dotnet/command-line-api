// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public class RelativeToEnvironmentVariableValueSource<T>(string environmentVariableName,
                                                         Func<string?, (bool success, T? value)>? calculation = null,
                                                         string? description = null)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override (bool success, T? value) GetTypedValue(PipelineResult pipelineResult)
    {
        string? stringValue = Environment.GetEnvironmentVariable(environmentVariableName);

        if (stringValue is null)
        {
            // This feels wrong. It isn't saying "Hey, you asked for a value that was not there"
            return default;
        }

        // TODO: What is the best way to do this?
        T value = default(T) switch
        {
            int i => (T)(object)Convert.ToInt32(stringValue),
            _ => throw new NotImplementedException("Looking for a non-dumb way to do this")
        };
        return calculation is null
            ? (true, value)
            : calculation(Environment.GetEnvironmentVariable(environmentVariableName));
    }
}

