// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class MethodBinderTests
    {
        [Theory]
        [InlineData(typeof(IConsole))]
        [InlineData(typeof(ITerminal))]
        [InlineData(typeof(InvocationContext))]
        [InlineData(typeof(ParseResult))]
        [InlineData(typeof(CancellationToken))]
        public void Options_are_not_built_for_infrastructure_types_exposed_by_method_parameters(Type type)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var target = Activator.CreateInstance(targetType);

            var binder = new MethodBinder(
                targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.Handle)),
                target);

            var options = binder.BuildOptions();

            options.Should()
                   .NotContain(o => o.Argument.ArgumentType == type);
        }

        public class ClassWithMethodHavingParameter<T>
        {
            public int Handle(T value)
            {
                return 0;
            }

            public Task<int> HandleAsync(T value)
            {
                return Task.FromResult(0);
            }
        }
    }
}
