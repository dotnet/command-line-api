// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.NamingConventionBinder.Tests;

public class BindingTestCase
{
    private readonly Action<object> _assertBoundValue;

    private BindingTestCase(
        string commandLineToken,
        Type parameterType,
        Action<object> assertBoundValue,
        string variationName) : this(
        new[] { commandLineToken },
        parameterType,
        assertBoundValue,
        variationName)
    {
    }

    private BindingTestCase(
        string[] commandLineTokens,
        Type parameterType,
        Action<object> assertBoundValue,
        string variationName)
    {
        _assertBoundValue = assertBoundValue;
        VariationName = variationName;
        CommandLineTokens = commandLineTokens;
        ParameterType = parameterType;
    }

    public string[] CommandLineTokens { get; }

    public Type ParameterType { get; }

    public string VariationName { get; }

    public void AssertBoundValue(object value)
    {
        _assertBoundValue(value);
    }

    public static BindingTestCase Create<T>(
        string commandLineToken,
        Action<T> assertBoundValue,
        string variationName = null) =>
        new(commandLineToken,
            typeof(T),
            o => assertBoundValue((T) o),
            variationName);

    public static BindingTestCase Create<T>(
        string[] commandLineTokens,
        Action<T> assertBoundValue,
        string variationName = null) =>
        new(commandLineTokens,
            typeof(T),
            o => assertBoundValue((T) o),
            variationName);
}