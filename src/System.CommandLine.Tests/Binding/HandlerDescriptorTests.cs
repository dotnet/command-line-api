// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class HandlerDescriptorTests
    {
        [Fact]
        public void Handler_descriptor_describes_the_parameter_names_of_the_handler_method()
        {
            var descriptor = HandlerDescriptor.FromExpression<ClassWithInvokeAndDefaultCtor, string, int, Task<int>>((model, s, i) => model.Invoke(s, i));

            descriptor.ParameterDescriptors
                      .Select(p => p.Name)
                      .Should()
                      .BeEquivalentSequenceTo("stringParam", "intParam");
        }

        [Fact]
        public void Handler_descriptor_describes_the_parameter_types_of_the_handler_method()
        {
            var descriptor = HandlerDescriptor.FromExpression<ClassWithInvokeAndDefaultCtor, string, int, Task<int>>((model, s, i) => model.Invoke(s, i));

            descriptor.ParameterDescriptors
                      .Select(p => p.Type)
                      .Should()
                      .BeEquivalentSequenceTo(typeof(string), typeof(int));
        }
    }
}
