// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class PropertyDescriptorTests
    {
        [Theory]
        [InlineData(typeof(string), "")]
        [InlineData(typeof(int), 0)]
        [InlineData(typeof(int?), null)]
        public void GetDefaultValue_returns_the_default_for_the_type(Type type, object defaultValue)
        {
            type = typeof(ClassWithSetter<>).MakeGenericType(type);

            var modelDescriptor = ModelDescriptor.FromType(type);

            modelDescriptor
                .PropertyDescriptors
                .Single()
                .GetDefaultValue()
                .Should()
                .Be(defaultValue);
        }
    }
}
