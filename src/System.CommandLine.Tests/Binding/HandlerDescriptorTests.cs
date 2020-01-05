// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class HandlerDescriptorTests
    {
        public class FromDelegate
        {
            [Theory]
            [InlineData(typeof(int))]
            [InlineData(typeof(string))]
            [InlineData(typeof(DirectoryInfo))]
            public void It_provides_the_names_of_the_handler_parameters(Type parameterType)
            {
                var method = typeof(HandlerDescriptorTests).GetMethod(nameof(Handler)).MakeGenericMethod(parameterType);

                var descriptor = HandlerDescriptor.FromMethodInfo(method);

                descriptor.ParameterDescriptors
                          .Select(p => p.ValueName)
                          .Should()
                          .BeEquivalentSequenceTo("value");
            }

            [Theory]
            [InlineData(typeof(int))]
            [InlineData(typeof(string))]
            [InlineData(typeof(DirectoryInfo))]
            public void It_provides_the_types_of_the_handler_parameters(Type parameterType)
            {
                var method = typeof(HandlerDescriptorTests).GetMethod(nameof(Handler)).MakeGenericMethod(parameterType);

                var descriptor = HandlerDescriptor.FromMethodInfo(method);

                descriptor.ParameterDescriptors
                          .Select(p => p.Type)
                          .Should()
                          .BeEquivalentSequenceTo(parameterType);
            }
        }

        public class FromMethodInfo
        {
            [Theory]
            [InlineData(typeof(int))]
            [InlineData(typeof(string))]
            [InlineData(typeof(DirectoryInfo))]
            public void It_provides_the_names_of_the_handler_parameters(Type parameterType)
            {
                var method = typeof(HandlerDescriptorTests).GetMethod(nameof(Handler)).MakeGenericMethod(parameterType);

                var descriptor = HandlerDescriptor.FromMethodInfo(method);

                descriptor.ParameterDescriptors
                          .Select(p => p.ValueName)
                          .Should()
                          .BeEquivalentSequenceTo("value");
            }

            [Theory]
            [InlineData(typeof(int))]
            [InlineData(typeof(string))]
            [InlineData(typeof(DirectoryInfo))]
            public void It_provides_the_types_of_the_handler_parameters(Type parameterType)
            {
                var method = typeof(HandlerDescriptorTests).GetMethod(nameof(Handler)).MakeGenericMethod(parameterType);

                var descriptor = HandlerDescriptor.FromMethodInfo(method);

                descriptor.ParameterDescriptors
                          .Select(p => p.Type)
                          .Should()
                          .BeEquivalentSequenceTo(parameterType);
            }
        }

        public void Handler<T>(T value)
        {
        }

        public class FromExpression
        {
            [Fact]
            public void Handler_descriptor_describes_the_parameter_names_of_the_handler_method()
            {
                var descriptor = HandlerDescriptor.FromExpression<ClassWithInvokeAndDefaultCtor, string, int, Task<int>>((model, s, i) => model.Invoke(s, i));

                descriptor.ParameterDescriptors
                          .Select(p => p.ValueName)
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
}
