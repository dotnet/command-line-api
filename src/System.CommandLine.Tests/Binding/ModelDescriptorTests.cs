// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Tests.Utility;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class ModelDescriptorTests
    {
        [Fact]
        public void Model_descriptor_describes_the_properties_of_the_model_type()
        {
            var descriptor = ModelDescriptor.FromType<ClassWithMultiLetterSetters>();

            descriptor.PropertyDescriptors
                      .Select(p => p.ValueName)
                      .Should()
                      .BeEquivalentTo(
                          nameof(ClassWithMultiLetterSetters.BoolOption),
                          nameof(ClassWithMultiLetterSetters.IntOption),
                          nameof(ClassWithMultiLetterSetters.StringOption));
        }

        [Fact]
        public void Model_descriptor_describes_the_constructor_parameters_of_the_model_type()
        {
            var descriptor = ModelDescriptor.FromType<ClassWithSettersAndCtorParametersWithDifferentNames>();

            descriptor.ConstructorDescriptors
                      .SelectMany(p => p.ParameterDescriptors)
                      .Select(p => p.ValueName)
                      .Should()
                      .BeEquivalentSequenceTo("i", "s", "b");
        }
    }
}
