// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class ModelBinderTests
    {
        [Theory]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        [InlineData(typeof(int?), "--value 123", 123)]
        [InlineData(typeof(int?), "", null)]
        public void Option_arguments_are_bound_by_name_to_constructor_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithCtorParameter<>).MakeGenericType(type);
            var binder = new ModelBinder(targetType);

            var command = new Command("the-command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 ArgumentType = type
                                             }
                              }
                          };

            var bindingContext = new BindingContext(command.Parse(commandLine));

            var instance = binder.CreateInstance(bindingContext);

            object valueReceivedValue = ((dynamic)instance).Value;

            valueReceivedValue.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(typeof(string), "hello", "hello")]
        [InlineData(typeof(int), "123", 123)]
        [InlineData(typeof(int?), "123", 123)]
        [InlineData(typeof(int?), "", null)]
        public void Command_arguments_are_bound_by_name_to_constructor_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithCtorParameter<>).MakeGenericType(type);
            var binder = new ModelBinder(targetType);

            var command = new Command("the-command")
                          {
                              Argument = new Argument
                                         {
                                             Name = "value",
                                             ArgumentType = type
                                         }
                          };

            var bindingContext = new BindingContext(command.Parse(commandLine));

            var instance = binder.CreateInstance(bindingContext);

            object valueReceivedValue = ((dynamic)instance).Value;

            valueReceivedValue.Should().Be(expectedValue);
        }

        [Fact]
        public void Explicitly_configured_default_values_can_be_bound_by_name_to_constructor_parameters()
        {
            var option = new Option("--string-option")
            {
                Argument = new Argument<string>(() => "the default")
            };

            var command = new Command("the-command");
            command.AddOption(option);
            var binder = new ModelBinder(typeof(ClassWithMultiLetterCtorParameters));

            var parser = new Parser(command);
            var bindingContext = new BindingContext(parser.Parse(""));

            var instance = (ClassWithMultiLetterCtorParameters)binder.CreateInstance(bindingContext);

            instance.StringOption.Should().Be("the default");
        }

        [Theory]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        [InlineData(typeof(int?), "--value 123", 123)]
        [InlineData(typeof(int?), "", null)]
        public void Option_arguments_are_bound_by_name_to_property_setters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithSetter<>).MakeGenericType(type);
            var binder = new ModelBinder(targetType);

            var command = new Command("the-command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 ArgumentType = type
                                             }
                              }
                          };
            var parser = new Parser(command);

            var bindingContext = new BindingContext(parser.Parse(commandLine));

            var instance = binder.CreateInstance(bindingContext);

            object valueReceivedValue = ((dynamic)instance).Value;

            valueReceivedValue.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(typeof(string), "hello", "hello")]
        [InlineData(typeof(int), "123", 123)]
        [InlineData(typeof(int?), "123", 123)]
        [InlineData(typeof(int?), "", null)]
        public void Command_arguments_are_bound_by_name_to_property_setters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithSetter<>).MakeGenericType(type);
            var binder = new ModelBinder(targetType);

            var command = new Command("the-command")
                          {
                              Argument = new Argument
                                         {
                                             Name = "value",
                                             ArgumentType = type
                                         }
                          };
            var parser = new Parser(command);

            var bindingContext = new BindingContext(parser.Parse(commandLine));

            var instance = binder.CreateInstance(bindingContext);

            object valueReceivedValue = ((dynamic)instance).Value;

            valueReceivedValue.Should().Be(expectedValue);
        }

        [Fact]
        public void Types_having_constructors_accepting_a_single_string_are_bound_using_the_handler_parameter_name()
        {
            var tempPath = Path.GetTempPath();

            var option = new Option("--value")
            {
                Argument = new Argument<DirectoryInfo>()
            };

            var command = new Command("the-command");
            command.AddOption(option);
            var binder = new ModelBinder(typeof(ClassWithCtorParameter<DirectoryInfo>));
            var bindingContext = new BindingContext(command.Parse($"--value \"{tempPath}\""));

            var instance = (ClassWithCtorParameter<DirectoryInfo>)binder.CreateInstance(bindingContext);

            instance.Value.FullName.Should().Be(tempPath);
        }

        [Fact]
        public void Explicitly_configured_default_values_can_be_bound_by_name_to_property_setters()
        {
            var argument = new Argument<string>(() => "the default");

            var option = new Option("--value")
            {
                Argument = argument
            };

            var command = new Command("the-command");
            command.AddOption(option);
            var binder = new ModelBinder(typeof(ClassWithSetter<string>));

            var parser = new Parser(command);
            var bindingContext = new BindingContext(parser.Parse(""));

            var instance = (ClassWithSetter<string>)binder.CreateInstance(bindingContext);

            instance.Value.Should().Be("the default");  
        }

        [Fact]
        public void Property_setters_with_no_default_value_and_no_matching_option_are_not_called()
        {
            var command = new Command("the-command")
                          {
                              new Option("--string-option")
                              {
                                  Argument = new Argument<string>()
                              }
                          };

            var binder = new ModelBinder(typeof(ClassWithSettersAndCtorParametersWithDifferentNames));

            var parser = new Parser(command);
            var bindingContext = new BindingContext(
                parser.Parse(""));

            var instance = (ClassWithSettersAndCtorParametersWithDifferentNames)binder.CreateInstance(bindingContext);

            instance.StringOption.Should().Be("the default");
        }

        [Fact]
        public void Parse_result_can_be_used_to_create_an_instance_without_doing_handler_invocation()
        {
            var parser = new Parser(new Command("the-command")
                                    {
                                        new Option("--int-option")
                                        {
                                            Argument = new Argument<int>()
                                        }
                                    });
            var bindingContext = new BindingContext(parser.Parse("the-command --int-option 123"));
            var binder = new ModelBinder(typeof(ClassWithMultiLetterSetters));

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact]
        public void Parse_result_can_be_used_to_modify_an_existing_instance_without_doing_handler_invocation()
        {
            var parser = new Parser(new Command("the-command")
                                    {
                                        new Option("--int-option")
                                        {
                                            Argument = new Argument<int>()
                                        }
                                    });
            var instance = new ClassWithMultiLetterSetters();
            var bindingContext = new BindingContext(parser.Parse("the-command --int-option 123"));
            var binder = new ModelBinder(typeof(ClassWithMultiLetterSetters));

            binder.UpdateInstance(instance, bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact]
        public void Values_from_options_on_parent_commands_are_bound_by_name_by_default()
        {
            var parentCommand = new Command("parent-command")
                                {
                                    new Option("--int-option")
                                    {
                                        Argument = new Argument<int>()
                                    },
                                    new Command("child-command")
                                };

            var binder = new ModelBinder<ClassWithMultiLetterSetters>();

            var parseResult = parentCommand.Parse("parent-command --int-option 123 child-command");

            var bindingContext = new BindingContext(parseResult);

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact]
        public void Values_from_parent_command_arguments_are_bound_by_name_by_default()
        {
            var parentCommand = new Command("parent-command")
            {
                new Argument<int>
                {
                    Name = nameof(ClassWithMultiLetterSetters.IntOption)
                },
                new Command("child-command")
            };

            var binder = new ModelBinder<ClassWithMultiLetterSetters>();

            var parseResult = parentCommand.Parse("parent-command 123 child-command");

            var bindingContext = new BindingContext(parseResult);

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact]
        public void Values_from_options_on_parent_commands_can_be_bound_regardless_of_naming()
        {
            var childCommand = new Command("child-command");
            var option = new Option("-x")
                         {
                             Argument = new Argument<int>()
                         };
            var parentCommand = new Command("parent-command")
                                {
                                    option,
                                    childCommand
                                };

            var binder = new ModelBinder<ClassWithMultiLetterSetters>();

            binder.BindMemberFromValue(
                c => c.IntOption,
                option);

            var bindingContext = new BindingContext(parentCommand.Parse("parent-command -x 123 child-command"));

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact]
        public void Arbitrary_values_can_be_bound()
        {
            var command = new Command("the-command");

            var binder = new ModelBinder<ClassWithMultiLetterSetters>();

            binder.BindMemberFromValue(
                c => c.IntOption,
                _ => 123);

            var bindingContext = new BindingContext(command.Parse("the-command"));

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact]
        public void PropertyInfo_can_be_bound_to_option()
        {
            var command = new Command("the-command");
            var option = new Option("--fred")
            {
                Argument = new Argument<int>()
            };
            command.Add(option);

            var type = typeof(ClassWithMultiLetterSetters);
            var binder = new ModelBinder(type);
            var propertyInfo = type.GetProperties().First();

            binder.BindMemberFromValue(
                propertyInfo,
                option);

            var bindingContext = new BindingContext(command.Parse("the-command --fred 42"));

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(42);
        }

        [Fact]
        public void PropertyInfo_can_be_bound_to_argument()
        {
            var command = new Command("the-command");
            var argument = new Argument<int> { Arity = ArgumentArity.ExactlyOne };
            command.AddArgument(argument);

            var type = typeof(ClassWithMultiLetterSetters);
            var binder = new ModelBinder(type);
            var propertyInfo = type.GetProperty(nameof(ClassWithMultiLetterSetters.IntOption));

            binder.BindMemberFromValue(propertyInfo, argument);

            var bindingContext = new BindingContext(command.Parse("the-command 42"));

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(42);
        }

        [Fact]
        public void PropertyExpression_can_be_bound_to_option()
        {
            var command = new Command("the-command");
            var option = new Option("--fred") { Argument = new Argument<int>() };
            command.AddOption(option);

            var binder = new ModelBinder<ClassWithMultiLetterSetters>();

            binder.BindMemberFromValue(
                i => i.IntOption,
                option);

            var bindingContext = new BindingContext(command.Parse("the-command --fred 42"));

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(42);
        }

        [Fact]
        public void PropertyExpression_can_be_bound_to_argument()
        {
            var command = new Command("the-command");
            var argument = new Argument<int> { Arity = ArgumentArity.ExactlyOne };
            command.AddArgument(argument);

            var binder = new ModelBinder<ClassWithMultiLetterSetters>();

            binder.BindMemberFromValue(
                i => i.IntOption,
                argument);

            var bindingContext = new BindingContext(command.Parse("the-command 42"));

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(42);
        }
    }
}
