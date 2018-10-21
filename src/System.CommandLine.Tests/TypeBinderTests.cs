// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class TypeBinderTests
    {
        [Fact]
        public void Single_character_constructor_arguments_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var binder = new TypeBinder(typeof(ClassWithSingleLetterCtorParameter));

            var options = binder.BuildOptions().ToArray();

            options.Should().Contain(o => o.HasRawAlias("-x"));
            options.Should().Contain(o => o.HasRawAlias("-y"));
        }

        [Fact]
        public void Multi_character_constructor_arguments_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var binder = new TypeBinder(typeof(ClassWithMultiLetterCtorParameter));

            var options = binder.BuildOptions().ToArray();

            options.Should().Contain(o => o.HasRawAlias("--int-option"));
            options.Should().Contain(o => o.HasRawAlias("--string-option"));
            options.Should().Contain(o => o.HasRawAlias("--boolean-option"));
        }

        [Fact]
        public void Single_character_setters_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var binder = new TypeBinder(typeof(ClassWithSingleLetterProperty));

            var options = binder.BuildOptions().ToArray();

            options.Should().Contain(o => o.HasRawAlias("-x"));
            options.Should().Contain(o => o.HasRawAlias("-y"));
        }

        [Fact]
        public void Multi_character_setters_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var binder = new TypeBinder(typeof(ClassWithMultiLetterProperty));

            var options = binder.BuildOptions().ToArray();

            options.Should().Contain(o => o.HasRawAlias("--int-option"));
            options.Should().Contain(o => o.HasRawAlias("--string-option"));
            options.Should().Contain(o => o.HasRawAlias("--boolean-option"));
        }

        public class ClassWithSingleLetterCtorParameter
        {
            public ClassWithSingleLetterCtorParameter(int x, string y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }

            public string Y { get; }
        }

        public class ClassWithSingleLetterProperty
        {
            public int X { get; set; }

            public int Y { get; set; }
        }

        public class ClassWithMultiLetterCtorParameter
        {
            public ClassWithMultiLetterCtorParameter(
                int intOption,
                string stringOption,
                bool booleanOption)
            {
                IntOption = intOption;
                StringOption = stringOption;
                BooleanOption = booleanOption;
            }

            int IntOption { get; }
            string StringOption { get; }
            bool BooleanOption { get; }
        }

        public class ClassWithMultiLetterProperty
        {
            public int IntOption { get; set; }
            public string StringOption { get; set; }
            public bool BooleanOption { get; set; }
        }
    }
}
