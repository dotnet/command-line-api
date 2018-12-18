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
            options.Should().Contain(o => o.HasRawAlias("--bool-option"));
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
            var binder = new TypeBinder(typeof(ClassWithMultiLetterSetters));

            var options = binder.BuildOptions().ToArray();

            options.Should().Contain(o => o.HasRawAlias("--int-option"));
            options.Should().Contain(o => o.HasRawAlias("--string-option"));
            options.Should().Contain(o => o.HasRawAlias("--bool-option"));
        }

        [Fact]
        public void When_both_constructor_parameters_and_setters_are_present_then_BuildOptions_creates_options_for_all_of_them()
        {
            var binder = new TypeBinder(typeof(ClassWithSettersAndCtorParametersWithDifferentNames));

            var options = binder.BuildOptions();

            options.Should().Contain(o => o.HasRawAlias("--int-option"));
            options.Should().Contain(o => o.HasRawAlias("--string-option"));
            options.Should().Contain(o => o.HasRawAlias("--bool-option"));

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

        [Fact]
        public void Parsed_values_can_be_bound_to_constructor_parameters()
        {
            var argument = new Argument<string>("the default");

            var option = new Option("--string-option",
                                    argument: argument);

            var command = new Command("the-command");
            command.AddOption(option);
            var binder = new TypeBinder(typeof(ClassWithMultiLetterCtorParameter));

            var parser = new Parser(command);
            var invocationContext = new InvocationContext(
                parser.Parse("--string-option not-the-default"),
                parser);

            var instance = (ClassWithMultiLetterCtorParameter)binder.CreateInstance(invocationContext);

            instance.StringOption.Should().Be("not-the-default");
        }

        [Fact]
        public void Explicitly_configured_default_values_can_be_bound_to_constructor_parameters()
        {
            var argument = new Argument<string>("the default");

            var option = new Option("--string-option",
                                    argument: argument);

            var command = new Command("the-command");
            command.AddOption(option);
            var binder = new TypeBinder(typeof(ClassWithMultiLetterCtorParameter));

            var parser = new Parser(command);
            var invocationContext = new InvocationContext(
                parser.Parse(""),
                parser);

            var instance = (ClassWithMultiLetterCtorParameter)binder.CreateInstance(invocationContext);

            instance.StringOption.Should().Be("the default");
        }

        [Fact]
        public void Parsed_values_can_be_bound_to_property_setters()
        {
            var argument = new Argument<bool>();

            var option = new Option("--bool-option",
                                    argument: argument);

            var command = new Command("the-command");
            command.AddOption(option);
            var binder = new TypeBinder(typeof(ClassWithMultiLetterSetters));

            var parser = new Parser(command);
            var invocationContext = new InvocationContext(
                parser.Parse("--bool-option"),
                parser);

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(invocationContext);

            instance.BoolOption.Should().BeTrue();
        }

        [Fact]
        public void Explicitly_configured_default_values_can_be_bound_to_property_setters()
        {
            var argument = new Argument<string>("the default");

            var option = new Option("--string-option",
                                    argument: argument);

            var command = new Command("the-command");
            command.AddOption(option);
            var binder = new TypeBinder(typeof(ClassWithMultiLetterSetters));

            var parser = new Parser(command);
            var invocationContext = new InvocationContext(
                parser.Parse(""),
                parser);

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(invocationContext);

            instance.StringOption.Should().Be("the default");
        }

        [Fact]
        public void Property_setters_with_no_default_value_and_no_matching_option_are_not_called()
        {
            var command = new Command("the-command");

            var binder = new TypeBinder(typeof(ClassWithSettersAndCtorParametersWithDifferentNames));

            foreach (var option in binder.BuildOptions())
            {
                command.Add(option);
            }

            var parser = new Parser(command);
            var invocationContext = new InvocationContext(
                parser.Parse(""),
                parser);

            var instance = (ClassWithSettersAndCtorParametersWithDifferentNames)binder.CreateInstance(invocationContext);

            instance.StringOption.Should().Be("the default");
        }

        [Theory]
        [InlineData(typeof(IConsole))]
        [InlineData(typeof(ITerminal))]
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
                bool boolOption = false)
            {
                IntOption = intOption;
                StringOption = stringOption;
                BoolOption = boolOption;
            }

            public int IntOption { get; }
            public string StringOption { get; }
            public bool BoolOption { get; }
        }

        public class ClassWithMultiLetterSetters
        {
            public int IntOption { get; set; }
            public string StringOption { get; set; }
            public bool BoolOption { get; set; }
        }

        public class ClassWithSettersAndCtorParametersWithDifferentNames
        {
            public ClassWithSettersAndCtorParametersWithDifferentNames(
                int i = 123,
                string s = "the default",
                bool b = false)
            {
                IntOption = i;
                StringOption = s;
                BoolOption = b;
            }

            public int IntOption { get; set; }
            public string StringOption { get; set; }
            public bool BoolOption { get; set; }
        }

        public class ClassWithSettersAndCtorParametersWithMatchingNames
        {
            public ClassWithSettersAndCtorParametersWithMatchingNames(
                int intOption = 123,
                string stringOption = "the default",
                bool boolOption = false)
            {
                IntOption = intOption;
                StringOption = stringOption;
                BoolOption = boolOption;
            }

            public int IntOption { get; set; }
            public string StringOption { get; set; }
            public bool BoolOption { get; set; }
        }

        public class ClassWithSetter<T>
        {
            public T Value { get; set; }
        }
    }
}
