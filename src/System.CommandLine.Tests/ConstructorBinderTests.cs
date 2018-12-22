// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ConstructorBinderTests
    {
        [Theory]
        [InlineData(typeof(IConsole))]
        [InlineData(typeof(InvocationContext))]
        [InlineData(typeof(ParseResult))]
        [InlineData(typeof(CancellationToken))]
        public void Options_are_not_built_for_infrastructure_types_exposed_by_constructor_parameters(Type type)
        {
            var binder = new TypeBinder(typeof(ClassWithCtorParameter<>).MakeGenericType(type));

            var options = binder.BuildOptions();

            options.Should()
                   .NotContain(o => o.Argument.ArgumentType == type);
        }

        public class ClassWithCtorParameter<T>
        {
            public T Value { get; }

            public ClassWithCtorParameter(T value)
            {
                Value = value;
            }
        }
    }
}
