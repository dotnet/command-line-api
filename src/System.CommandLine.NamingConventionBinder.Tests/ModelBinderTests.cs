// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Binding;
using System.CommandLine.Utility;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.NamingConventionBinder.Tests;

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

        var command = new CliCommand("the-command")
        {
            OptionBuilder.CreateOption("--value", type)
        };

        var bindingContext = command.Parse(commandLine).GetBindingContext();

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

        var command = new CliCommand("the-command")
        {
            ArgumentBuilder.CreateArgument(type)
        };

        var bindingContext = command.Parse(commandLine).GetBindingContext();

        var instance = binder.CreateInstance(bindingContext);

        object valueReceivedValue = ((dynamic)instance).Value;

        valueReceivedValue.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(typeof(FileInfo), "MyFile.cs")]
    public void Command_arguments_are_bound_by_name_to_complex_constructor_parameters(
        Type type,
        string commandLine)
    {
        var targetType = typeof(ClassWithCtorParameter<>).MakeGenericType(type);
        var binder = new ModelBinder(targetType);

        var command = new CliCommand("the-command")
        {
            ArgumentBuilder.CreateArgument(type)
        };

        var bindingContext = command.Parse(commandLine).GetBindingContext();

        var instance = binder.CreateInstance(bindingContext);

        object valueReceivedValue = ((dynamic)instance).Value;
        var expectedValue = new FileInfo(commandLine);

        valueReceivedValue.Should().BeOfType<FileInfo>();
        var fileInfoValue = valueReceivedValue as FileInfo;
        fileInfoValue.FullName.Should().Be(expectedValue.FullName);
    }

    [Fact]
    public void Explicitly_configured_default_values_can_be_bound_by_name_to_constructor_parameters()
    {
        var option = new CliOption<string>("--string-option")
        {
            DefaultValueFactory = (_) => "the default",
        };

        var command = new CliCommand("the-command");
        command.Options.Add(option);
        var binder = new ModelBinder(typeof(ClassWithMultiLetterCtorParameters));

        var bindingContext = CliParser.Parse(command, "").GetBindingContext();

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

        var command = new CliCommand("the-command")
        {
            OptionBuilder.CreateOption("--value", type)
        };

        var bindingContext = command.Parse(commandLine).GetBindingContext();

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

        var command = new CliCommand("the-command")
        {
            ArgumentBuilder.CreateArgument(type)
        };

        var bindingContext = command.Parse(commandLine).GetBindingContext();

        var instance = binder.CreateInstance(bindingContext);

        object valueReceivedValue = ((dynamic)instance).Value;

        valueReceivedValue.Should().Be(expectedValue);
    }

    [Fact]
    public void Types_having_constructors_accepting_a_single_string_are_bound_using_the_handler_parameter_name()
    {
        var tempPath = Path.GetTempPath();

        var option = new CliOption<DirectoryInfo>("--value");

        var command = new CliCommand("the-command");
        command.Options.Add(option);
        var binder = new ModelBinder(typeof(ClassWithCtorParameter<DirectoryInfo>));
        var bindingContext = command.Parse($"--value \"{tempPath}\"").GetBindingContext();

        var instance = (ClassWithCtorParameter<DirectoryInfo>)binder.CreateInstance(bindingContext);

        instance.Value.FullName.Should().Be(tempPath);
    }

    [Fact]
    public void Explicitly_configured_default_values_can_be_bound_by_name_to_property_setters()
    {
        var option = new CliOption<string>("--value") { DefaultValueFactory = (_) => "the default" };

        var command = new CliCommand("the-command");
        command.Options.Add(option);
        var binder = new ModelBinder(typeof(ClassWithSetter<string>));

        var bindingContext = command.Parse("").GetBindingContext();

        var instance = (ClassWithSetter<string>)binder.CreateInstance(bindingContext);

        instance.Value.Should().Be("the default");
    }

    [Fact]
    public void Property_setters_with_no_default_value_and_no_matching_option_are_not_called()
    {
        var command = new CliCommand("the-command")
        {
            new CliOption<string>("--string-option")
        };

        var binder = new ModelBinder(typeof(ClassWithSettersAndCtorParametersWithDifferentNames));

        var bindingContext = command.Parse("").GetBindingContext();

        var instance = (ClassWithSettersAndCtorParametersWithDifferentNames)binder.CreateInstance(bindingContext);

        instance.StringOption.Should().Be("the default");
    }

    [Fact]
    public void Parse_result_can_be_used_to_create_an_instance_without_doing_handler_invocation()
    {
        CliCommand command = new ("the-command")
        {
            new CliOption<int>("--int-option")
        };
        var bindingContext = command.Parse("the-command --int-option 123").GetBindingContext();
        var binder = new ModelBinder(typeof(ClassWithMultiLetterSetters));

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Parse_result_can_be_used_to_modify_an_existing_instance_without_doing_handler_invocation()
    {
        CliCommand command = new("the-command")
        {
            new CliOption<int>("--int-option")
        };
        var instance = new ClassWithMultiLetterSetters();
        var bindingContext = command.Parse("the-command --int-option 123").GetBindingContext();
        var binder = new ModelBinder(typeof(ClassWithMultiLetterSetters));

        binder.UpdateInstance(instance, bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Modify_an_existing_instance_should_keep_all_default_values_if_no_argument_matches_option()
    {
        CliCommand parser = new ("the-command");

        var instance = new ClassWithComplexTypes();
        var bindingContext = parser.Parse("the-command").GetBindingContext();
        var binder = new ModelBinder(typeof(ClassWithComplexTypes));

        binder.UpdateInstance(instance, bindingContext);

        instance.Should().BeEquivalentTo(new ClassWithComplexTypes());
    }

    [Fact]
    public void Values_from_options_on_parent_commands_are_bound_by_name_by_default()
    {
        var parentCommand = new CliCommand("parent-command")
        {
            new CliOption<int>("--int-option"),
            new CliCommand("child-command")
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        var parseResult = parentCommand.Parse("parent-command --int-option 123 child-command");

        var bindingContext = parseResult.GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Default_values_from_options_on_parent_commands_are_bound_by_name_by_default()
    {
        var parentCommand = new CliCommand("parent-command")
        {
            new CliOption<int>("--int-option")
            { 
                DefaultValueFactory = (_) => 123,
            },
            new CliCommand("child-command")
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        var parseResult = parentCommand.Parse("parent-command child-command");

        var bindingContext = parseResult.GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Values_from_parent_command_arguments_are_bound_by_name_by_default()
    {
        var parentCommand = new CliCommand("parent-command")
        {
            new CliArgument<int>(nameof(ClassWithMultiLetterSetters.IntOption)),
            new CliCommand("child-command")
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        var parseResult = parentCommand.Parse("parent-command 123 child-command");

        var bindingContext = parseResult.GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Default_values_from_parent_command_arguments_are_bound_by_name_by_default()
    {
        var parentCommand = new CliCommand("parent-command")
        {
            new CliArgument<int>(nameof(ClassWithMultiLetterSetters.IntOption))
            {
                DefaultValueFactory = (_) => 123
            },
            new CliCommand("child-command")
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        var parseResult = parentCommand.Parse("parent-command child-command");

        var bindingContext = parseResult.GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Values_from_options_on_parent_commands_can_be_bound_regardless_of_naming()
    {
        var childCommand = new CliCommand("child-command");
        var option = new CliOption<int>("-x");
        var parentCommand = new CliCommand("parent-command")
        {
            option,
            childCommand
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        binder.BindMemberFromValue(
            c => c.IntOption,
            option);

        var bindingContext = parentCommand.Parse("parent-command -x 123 child-command").GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Arbitrary_values_can_be_bound()
    {
        var command = new CliCommand("the-command");

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        binder.BindMemberFromValue(
            c => c.IntOption,
            _ => 123);

        var bindingContext = command.Parse("the-command").GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void PropertyInfo_can_be_bound_to_option()
    {
        var command = new CliCommand("the-command");
        var option = new CliOption<int>("--fred");
        command.Add(option);

        var type = typeof(ClassWithMultiLetterSetters);
        var binder = new ModelBinder(type);
        var propertyInfo = type.GetProperties()[0];

        binder.BindMemberFromValue(
            propertyInfo,
            option);

        var bindingContext = command.Parse("the-command --fred 42").GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(42);
    }

    [Fact]
    public void PropertyInfo_can_be_bound_to_argument()
    {
        var command = new CliCommand("the-command");
        var argument = new CliArgument<int>("arg") { Arity = ArgumentArity.ExactlyOne };
        command.Arguments.Add(argument);

        var type = typeof(ClassWithMultiLetterSetters);
        var binder = new ModelBinder(type);
        var propertyInfo = type.GetProperty(nameof(ClassWithMultiLetterSetters.IntOption));

        binder.BindMemberFromValue(propertyInfo, argument);

        var bindingContext = command.Parse("the-command 42").GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(42);
    }

    [Fact]
    public void PropertyExpression_can_be_bound_to_option()
    {
        var command = new CliCommand("the-command");
        var option = new CliOption<int>("--fred");
        command.Options.Add(option);

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        binder.BindMemberFromValue(
            i => i.IntOption,
            option);

        var bindingContext = command.Parse("the-command --fred 42").GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(42);
    }

    [Fact]
    public void PropertyExpression_can_be_bound_to_argument()
    {
        var command = new CliCommand("the-command");
        var argument = new CliArgument<int>("arg") { Arity = ArgumentArity.ExactlyOne };
        command.Arguments.Add(argument);

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        binder.BindMemberFromValue(
            i => i.IntOption,
            argument);

        var bindingContext = command.Parse("the-command 42").GetBindingContext();

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(42);
    }

    [Fact]
    public void Option_argument_is_bound_to_longest_constructor()
    {
        var option = new CliOption<int>("--int-property");

        var bindingContext = new CliRootCommand { option }.Parse("--int-property 42").GetBindingContext();
        var binder = new ModelBinder<ClassWithMultipleCtor>();
        var instance = binder.CreateInstance(bindingContext) as ClassWithMultipleCtor;

        instance.Should().NotBeNull();
        instance.IntProperty.Should().Be(42);
    }

    [Fact]
    public void Command_argument_is_bound_to_longest_constructor()
    {
        var rootCommand = new CliRootCommand();
        rootCommand.Arguments.Add(new CliArgument<int>(nameof(ClassWithMultipleCtor.IntProperty)));

        var bindingContext = rootCommand.Parse("42").GetBindingContext();
        var binder = new ModelBinder<ClassWithMultipleCtor>();
        var instance = binder.CreateInstance(bindingContext) as ClassWithMultipleCtor;

        instance.Should().NotBeNull();
        instance.IntProperty.Should().Be(42);
    }

    [Fact]
    public void Explicit_model_binder_binds_only_to_configured_properties()
    {
        var intOption = new CliOption<int>("--int-property");
        var stringOption = new CliOption<string>("--string-property");
        CliRootCommand rootCommand = new CliRootCommand { intOption, stringOption };

        var bindingContext = rootCommand.Parse("--int-property 42 --string-property Hello").GetBindingContext();
        var binder = new ModelBinder<ClassWithMultiLetterSetters>
        {
            EnforceExplicitBinding = true
        };
        binder.BindMemberFromValue(obj => obj.IntOption, intOption);
        var instance = binder.CreateInstance(bindingContext) as ClassWithMultiLetterSetters;

        instance.Should().NotBeNull();
        instance.IntOption.Should().Be(42);
        instance.StringOption.Should().BeNull();
    }

    [Fact]
    public async Task Bound_array_command_arguments_default_to_an_empty_array_when_not_specified()
    {
        var rootCommand = new CliRootCommand("Command")
        {
            new CliArgument<string[]>("names")
        };
        rootCommand.Action = CommandHandler.Create<string[]>(Handler);
        string[] passedNames = null;
        await rootCommand.Parse("").InvokeAsync();

        passedNames.Should().BeEmpty();

        int Handler(string[] names)
        {
            passedNames = names;
            return 0;
        }
    }

    [Fact]
    public async Task Bound_enumerable_command_arguments_default_to_an_empty_array_when_not_specified()
    {
        var rootCommand = new CliRootCommand("Command")
        {
            new CliArgument<IEnumerable<string>>("names")
        };
        rootCommand.Action = CommandHandler.Create<IEnumerable<string>>(Handler);
        IEnumerable<string> passedNames = null;
        await rootCommand.Parse("").InvokeAsync();

        passedNames.Should().BeEmpty();

        int Handler(IEnumerable<string> names)
        {
            passedNames = names;
            return 0;
        }
    }

    [Fact]
    public async Task Bound_array_options_default_to_an_empty_array_when_not_specified()
    {
        var rootCommand = new CliRootCommand("Command")
        {
            new CliOption<string[]>("--names")
        };
        rootCommand.Action = CommandHandler.Create<string[]>(Handler);
        string[] passedNames = null;
        await rootCommand.Parse("").InvokeAsync();

        passedNames.Should().BeEmpty();

        int Handler(string[] names)
        {
            passedNames = names;
            return 0;
        }
    }

    [Fact]
    public async Task Bound_enumerable_options_default_to_an_empty_array_when_not_specified()
    {
        var rootCommand = new CliRootCommand("Command")
        {
            new CliOption<IEnumerable<string>>("--names"),
        };
        rootCommand.Action = CommandHandler.Create<IEnumerable<string>>(Handler);
        IEnumerable<string> passedNames = null;
        await rootCommand.Parse("").InvokeAsync();

        passedNames.Should().BeEmpty();

        int Handler(IEnumerable<string> names)
        {
            passedNames = names;
            return 0;
        }
    }

    [Fact]
    public void Default_values_from_options_with_the_same_type_are_bound_and_use_their_own_defaults()
    {
        int first = 0, second = 0;

        var rootCommand = new CliRootCommand
        {
            new CliOption<int>("one") { DefaultValueFactory = (_) => 1 },
            new CliOption<int>("two") { DefaultValueFactory = (_) => 2 }
        };
        rootCommand.Action = CommandHandler.Create<int, int>((one, two) =>
        {
            first = one;
            second = two;
        });

        var config = new CliConfiguration(rootCommand);

        config.Invoke("");

        first.Should().Be(1);
        second.Should().Be(2);
    }

    [Fact]
    public void Binder_does_not_match_on_partial_name()
    {
        var command = new CliRootCommand
        {
            new CliOption<List<string>>("--abc")
        };

        ClassWithOnePropertyNameThatIsSubstringOfAnother boundValue = default;

        command.Action = CommandHandler.Create(
            (ClassWithOnePropertyNameThatIsSubstringOfAnother s) => { boundValue = s; }
        );

        command.Parse(new[] { "--abc", "1" }).Invoke();

        boundValue.Abc
                  .Should()
                  .ContainSingle()
                  .Which
                  .Should()
                  .Be("1");
    }

    [Fact]
    public async Task Empty_input_is_bound_correctly_to_list_type_properties()
    {
        ClassWithListTypePropertiesAndDefaultCtor boundInstance = default;

        var cmd = new CliRootCommand
        {
            Action = CommandHandler.Create((ClassWithListTypePropertiesAndDefaultCtor value) => { boundInstance = value; })
        };

        var result = cmd.Parse(string.Empty);

        await result.InvokeAsync();

        boundInstance.Should().NotBeNull();
    }

    [Fact]
    public async Task Decimals_are_bound_correctly_when_no_token_is_matched()
    {
        decimal? receivedValue = null;

        var rootCommand = new CliRootCommand
        {
            new CliOption<decimal>("--opt-decimal")
        };
        rootCommand.Action = CommandHandler.Create((ComplexType options) => { receivedValue = options.OptDecimal; });

        await rootCommand.Parse("").InvokeAsync();

        receivedValue.Should().Be(0);
    }

    public class ComplexType
    {
        public decimal OptDecimal { get; set; }
    }

    [Fact] // issue: https://github.com/dotnet/command-line-api/issues/1365
    public void Binder_does_not_match_by_substring()
    {
        var rootCommand = new CliRootCommand
        {
            new CliOption<string>("--bundle", "-b")
            { 
                Description = "the path to the app bundle to be installed"
            },
            new CliOption<string>("--bundle-id", "--bundle_id", "-1")
            {
                Description = "specify bundle id for list and upload"
            }
        };

        DeployOptions boundOptions = null;

        rootCommand.Action = CommandHandler.Create<DeployOptions>(options =>
        {
            boundOptions = options;
            return 0;
        });

        rootCommand.Parse("-1 value").Invoke();

        boundOptions.Bundle.Should().Be(null);
        boundOptions.BundleId.Should().Be("value");
    }

    [Fact]
    public void ParseResult_GetValue_with_generic_option_returns_value()
    {
        CliOption<int> option = new("--number");
        CliCommand command = new("the-command")
        {
            option
        };

        ParseResult parseResult = command.Parse("the-command --number 42");

        parseResult.GetValue(option)
            .Should()
            .Be(42);
    }

    [Fact]
    public void ParseResult_GetValue_with_generic_argument_returns_value()
    {
        CliArgument<int> argument = new("arg");
        CliCommand command = new("the-command")
        {
            argument
        };

        ParseResult parseResult = command.Parse("the-command 42");

        parseResult.GetValue(argument)
            .Should()
            .Be(42);
    }

    class DeployOptions
    {
        public string Bundle { get; set; }
        public string BundleId { get; set; }
    }

#if NETCOREAPP2_1_OR_GREATER
    [Theory]
    [InlineData("--class-with-span-ctor a51ca309-84fa-452f-96be-51e47702ffb4 --int-value 1234")]
    [InlineData("--class-with-span-ctor a51ca309-84fa-452f-96be-51e47702ffb4")]
    [InlineData("--int-value 1234")]
    public void When_only_available_constructor_is_span_then_null_is_passed(string commandLine)
    {
        var root = new CliRootCommand
        {
            new CliOption<ClassWithSpanConstructor>("--class-with-span-ctor"),
            new CliOption<int>("--int-value"),
        };

        var handlerWasCalled = false;

        root.Action = CommandHandler.Create<ClassWithSpanConstructor, int>((spanCtor, intValue) =>
        {
            handlerWasCalled = true;
        });

        root.Parse(commandLine).Invoke();

        handlerWasCalled.Should().BeTrue();
    }

    public class ClassWithSpanConstructor
    {
        private Guid value;

        public ClassWithSpanConstructor(ReadOnlySpan<byte> guid)
        {
            value = new Guid(guid);
        }

        public override string ToString() => value.ToString();
    }
#endif
}