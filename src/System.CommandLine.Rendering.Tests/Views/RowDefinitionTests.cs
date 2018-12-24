// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering.Views;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests.Views
{
    public class RowDefinitionTests
    {
        [Fact]
        public void Star_sized_row_definition_negative_weight_throws()
        {
            Action createNegativeWeight = () => RowDefinition.Star(-1);
            createNegativeWeight.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Star_sized_row_definition_can_be_zero()
        {
            var rowDefinition = RowDefinition.Star(0);
            rowDefinition.Value.Should().Be(0);
            rowDefinition.SizeMode.Should().Be(SizeMode.Star);
        }

        [Fact]
        public void Can_create_star_sized_row_definition()
        {
            var rowDefinition = RowDefinition.Star(1);
            rowDefinition.Value.Should().Be(1.0);
            rowDefinition.SizeMode.Should().Be(SizeMode.Star);
        }

        [Fact]
        public void Fixed_sized_row_definition_negative_weight_throws()
        {
            Action createNegativeWeight = () => RowDefinition.Fixed(-1);
            createNegativeWeight.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Fixed_sized_row_definition_can_be_zero()
        {
            var rowDefinition = RowDefinition.Fixed(0);
            rowDefinition.Value.Should().Be(0);
            rowDefinition.SizeMode.Should().Be(SizeMode.Fixed);
        }

        [Fact]
        public void Can_create_fixed_sized_row_definition()
        {
            var rowDefinition = RowDefinition.Fixed(1);
            rowDefinition.Value.Should().Be(1.0);
            rowDefinition.SizeMode.Should().Be(SizeMode.Fixed);
        }

        [Fact]
        public void Can_create_size_to_content_row_definition()
        {
            var rowDefinition = RowDefinition.SizeToContent();
            rowDefinition.Value.Should().Be(0.0);
            rowDefinition.SizeMode.Should().Be(SizeMode.SizeToContent);
        }
    }
}
