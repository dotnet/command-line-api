// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Binding;
using System.CommandLine.Tests.Utility;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.NamingConventionBinder.Tests;

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