// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Tests.Binding
{
    public class BindingTestCase
    {
        private readonly Action<object> _assertBoundValue;

        public BindingTestCase(
            string commandLine,
            Type parameterType,
            Action<object> assertBoundValue)
        {
            _assertBoundValue = assertBoundValue;
            CommandLine = commandLine;
            ParameterType = parameterType;
        }

        public string CommandLine { get; }

        public Type ParameterType { get; }

        public void AssertBoundValue(object value)
        {
            _assertBoundValue(value);
        }

        public static BindingTestCase Create<T>(
            string commandLine,
            Action<T> assertBoundValue) =>
            new BindingTestCase(
                commandLine,
                typeof(T),
                o => assertBoundValue((T)o)
            );
    }

    public class BindingTestSet : Dictionary<Type, BindingTestCase>
    {
        public void Add(BindingTestCase testCase) => Add(testCase.ParameterType, testCase);
    }
}
