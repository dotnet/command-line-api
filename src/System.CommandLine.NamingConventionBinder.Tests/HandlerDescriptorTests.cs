// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.NamingConventionBinder.Tests;

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
                      .Select(p => p.ValueType)
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
                      .Select(p => p.ValueType)
                      .Should()
                      .BeEquivalentSequenceTo(parameterType);
        }
    }

    public void Handler<T>(T value)
    {
    }
}