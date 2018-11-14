// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using FluentAssertions;
using System.Linq;
using System.Threading;
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
        public void Multi_character_constructor_arguments_generate_aliases_that_accept_a_double_dash_prefix()
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

        [Fact]
        public void When_both_constructor_parameters_and_setters_are_present_then_BuildOptions_creates_options_for_all_of_them()
        {
            var binder = new TypeBinder(typeof(ClassWithSettersAndCtorParameters));

            var options = binder.BuildOptions();

            options.Should().Contain(o => o.HasRawAlias("--int-option"));
            options.Should().Contain(o => o.HasRawAlias("--string-option"));
            options.Should().Contain(o => o.HasRawAlias("--boolean-option"));

            options.Should().Contain(o => o.HasRawAlias("-i"));
            options.Should().Contain(o => o.HasRawAlias("-s"));
            options.Should().Contain(o => o.HasRawAlias("-b"));
        }

        [Fact]
        public void Default_option_values_are_based_on_constructor_parameter_defaults()
        {
            var binder = new TypeBinder(typeof(ClassWithMultiLetterCtorParameter));

            var options = binder.BuildOptions().ToArray();

            options.Single(o => o.HasRawAlias("--int-option"))
                   .Argument
                   .GetDefaultValue()
                   .Should()
                   .Be(123);

            options.Single(o => o.HasRawAlias("--string-option"))
                   .Argument
                   .GetDefaultValue()
                   .Should()
                   .Be("the default");
        }

        [Theory]
        [InlineData(typeof(IConsole))]
        [InlineData(typeof(InvocationContext))]
        [InlineData(typeof(ParseResult))]
        [InlineData(typeof(CancellationToken))]
        public void Options_are_not_built_for_infrastructure_types_exposed_by_properties(Type type)
        {
            var binder = new TypeBinder(typeof(ClassWithSetter<>).MakeGenericType(type));

            var options = binder.BuildOptions();

            options.Should()
                   .NotContain(o => o.Argument.ArgumentType == type);
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
                int intOption = 123,
                string stringOption = "the default",
                bool booleanOption = false)
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

        public class ClassWithSettersAndCtorParameters
        {
            public ClassWithSettersAndCtorParameters(
                int i = 123,
                string s = "the default",
                bool b = false)
            {
                IntOption = i;
                StringOption = s;
                BooleanOption = b;
            }

            public int IntOption { get; set; }
            public string StringOption { get; set; }
            public bool BooleanOption { get; set; }
        }

        public class ClassWithSetter<T>
        {
            public T Value { get; set; }
        }
    }
}
