// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Tests.Binding
{
    public class BindingTestCase
    {
        private readonly Action<object> _assertBoundValue;
        
        private BindingTestCase(
            string commandLine,
            Type parameterType,
            Action<object> assertBoundValue,
            string variationName)
        {
            _assertBoundValue = assertBoundValue;
            VariationName = variationName;
            CommandLine = commandLine;
            ParameterType = parameterType;
        }

        public string CommandLine { get; }

        public Type ParameterType { get; }

        public string VariationName { get; }

        public void AssertBoundValue(object value)
        {
            _assertBoundValue(value);
        }

        public static BindingTestCase Create<T>(
            string commandLine,
            Action<T> assertBoundValue, 
            string variationName = null) =>
            new BindingTestCase(
                commandLine,
                typeof(T),
                o => assertBoundValue((T) o),
                variationName);
    }
}