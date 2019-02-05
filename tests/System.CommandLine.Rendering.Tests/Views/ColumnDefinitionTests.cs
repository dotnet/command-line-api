// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering.Views;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests.Views
{
    public class ColumnDefinitionTests
    {
        [Fact]
        public void Star_sized_column_definition_negative_weight_throws()
        {
            Action createNegativeWeight = () => ColumnDefinition.Star(-1);
            createNegativeWeight.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Star_sized_column_definition_can_be_zero()
        {
            var columnDefinition = ColumnDefinition.Star(0);
            columnDefinition.Value.Should().Be(0);
            columnDefinition.SizeMode.Should().Be(SizeMode.Star);
        }

        [Fact]
        public void Can_create_star_sized_column_definition()
        {
            var columnDefinition = ColumnDefinition.Star(1);
            columnDefinition.Value.Should().Be(1.0);
            columnDefinition.SizeMode.Should().Be(SizeMode.Star);
        }

        [Fact]
        public void Fixed_sized_column_definition_negative_weight_throws()
        {
            Action createNegativeWeight = () => ColumnDefinition.Fixed(-1);
            createNegativeWeight.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Fixed_sized_column_definition_can_be_zero()
        {
            var columnDefinition = ColumnDefinition.Fixed(0);
            columnDefinition.Value.Should().Be(0);
            columnDefinition.SizeMode.Should().Be(SizeMode.Fixed);
        }

        [Fact]
        public void Can_create_fixed_sized_column_definition()
        {
            var columnDefinition = ColumnDefinition.Fixed(1);
            columnDefinition.Value.Should().Be(1.0);
            columnDefinition.SizeMode.Should().Be(SizeMode.Fixed);
        }

        [Fact]
        public void Can_create_size_to_content_column_definition()
        {
            var columnDefinition = ColumnDefinition.SizeToContent();
            columnDefinition.Value.Should().Be(0.0);
            columnDefinition.SizeMode.Should().Be(SizeMode.SizeToContent);
        }
    }
}
