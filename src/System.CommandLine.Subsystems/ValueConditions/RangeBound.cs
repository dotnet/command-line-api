// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueConditions;

public class RangeBound<T>(ValueSource<T> valueSource, bool exclusive = false)
{

    public static implicit operator RangeBound<T>(T value) => RangeBound<T>.Create(value);
    public static implicit operator RangeBound<T>(Func<T> calculated) =>  RangeBound<T>.Create(calculated);

    public static RangeBound<T> Create(T value, string? description = null) 
        => new(new SimpleValueSource<T>(value, description));

    public static RangeBound<T> Create(Func<T> calculation, string? description = null) 
        => new(new CalculatedValueSource<T>(calculation));

    public static RangeBound<T> Create(CliValueSymbol otherSymbol, Func<object, T> calculation, string? description = null) 
        => new(new RelativeToSymbolValueSource<T>(otherSymbol, calculation, description));

    public static RangeBound<T> Create(string environmentVariableName, Func<string, T> calculation, string? description = null) 
        => new(new RelativeToEnvironmentVariableValueSource<T>(environmentVariableName, calculation, description));

    public ValueSource<T> ValueSource { get; } = valueSource;
    public bool Exclusive { get; } = exclusive;
}
