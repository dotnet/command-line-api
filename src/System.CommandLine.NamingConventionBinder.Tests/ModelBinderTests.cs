// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
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

        var command = new Command("the-command")
        {
            OptionBuilder.CreateOption("--value", type)
        };

        var bindingContext = new InvocationContext(command.Parse(commandLine)).BindingContext;

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
            ArgumentBuilder.CreateArgument(type)
        };

        var bindingContext = new InvocationContext(command.Parse(commandLine)).BindingContext;

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

        var command = new Command("the-command")
        {
            ArgumentBuilder.CreateArgument(type)
        };

        var bindingContext = new InvocationContext(command.Parse(commandLine)).BindingContext;

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
        var option = new Option<string>("--string-option", () => "the default");

        var command = new Command("the-command");
        command.AddOption(option);
        var binder = new ModelBinder(typeof(ClassWithMultiLetterCtorParameters));

        var parser = new Parser(command);
        var bindingContext = new InvocationContext(parser.Parse("")).BindingContext;

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
            OptionBuilder.CreateOption("--value", type)
        };
        var parser = new Parser(command);

        var bindingContext = new InvocationContext(parser.Parse(commandLine)).BindingContext;

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
            ArgumentBuilder.CreateArgument(type)
        };
        var parser = new Parser(command);

        var bindingContext = new InvocationContext(parser.Parse(commandLine)).BindingContext;

        var instance = binder.CreateInstance(bindingContext);

        object valueReceivedValue = ((dynamic)instance).Value;

        valueReceivedValue.Should().Be(expectedValue);
    }

    [Fact]
    public void Types_having_constructors_accepting_a_single_string_are_bound_using_the_handler_parameter_name()
    {
        var tempPath = Path.GetTempPath();

        var option = new Option<DirectoryInfo>("--value");

        var command = new Command("the-command");
        command.AddOption(option);
        var binder = new ModelBinder(typeof(ClassWithCtorParameter<DirectoryInfo>));
        var bindingContext = new InvocationContext(command.Parse($"--value \"{tempPath}\"")).BindingContext;

        var instance = (ClassWithCtorParameter<DirectoryInfo>)binder.CreateInstance(bindingContext);

        instance.Value.FullName.Should().Be(tempPath);
    }

    [Fact]
    public void Explicitly_configured_default_values_can_be_bound_by_name_to_property_setters()
    {
        var option = new Option<string>("--value", () => "the default");

        var command = new Command("the-command");
        command.AddOption(option);
        var binder = new ModelBinder(typeof(ClassWithSetter<string>));

        var parser = new Parser(command);
        var bindingContext = new InvocationContext(parser.Parse("")).BindingContext;

        var instance = (ClassWithSetter<string>)binder.CreateInstance(bindingContext);

        instance.Value.Should().Be("the default");
    }

    [Fact]
    public void Property_setters_with_no_default_value_and_no_matching_option_are_not_called()
    {
        var command = new Command("the-command")
        {
            new Option<string>("--string-option")
        };

        var binder = new ModelBinder(typeof(ClassWithSettersAndCtorParametersWithDifferentNames));

        var parser = new Parser(command);
        var bindingContext = new InvocationContext(
            parser.Parse("")).BindingContext;

        var instance = (ClassWithSettersAndCtorParametersWithDifferentNames)binder.CreateInstance(bindingContext);

        instance.StringOption.Should().Be("the default");
    }

    [Fact]
    public void Parse_result_can_be_used_to_create_an_instance_without_doing_handler_invocation()
    {
        var parser = new Parser(new Command("the-command")
        {
            new Option<int>("--int-option")
        });
        var bindingContext = new InvocationContext(parser.Parse("the-command --int-option 123")).BindingContext;
        var binder = new ModelBinder(typeof(ClassWithMultiLetterSetters));

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Parse_result_can_be_used_to_modify_an_existing_instance_without_doing_handler_invocation()
    {
        var parser = new Parser(new Command("the-command")
        {
            new Option<int>("--int-option")
        });
        var instance = new ClassWithMultiLetterSetters();
        var bindingContext = new InvocationContext(parser.Parse("the-command --int-option 123")).BindingContext;
        var binder = new ModelBinder(typeof(ClassWithMultiLetterSetters));

        binder.UpdateInstance(instance, bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Modify_an_existing_instance_should_keep_all_default_values_if_no_argument_matches_option()
    {
        var parser = new Parser(new Command("the-command"));

        var instance = new ClassWithComplexTypes();
        var bindingContext = new InvocationContext(parser.Parse("the-command")).BindingContext;
        var binder = new ModelBinder(typeof(ClassWithComplexTypes));

        binder.UpdateInstance(instance, bindingContext);

        instance.Should().BeEquivalentTo(new ClassWithComplexTypes());
    }

    [Fact]
    public void Values_from_options_on_parent_commands_are_bound_by_name_by_default()
    {
        var parentCommand = new Command("parent-command")
        {
            new Option<int>("--int-option"),
            new Command("child-command")
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        var parseResult = parentCommand.Parse("parent-command --int-option 123 child-command");

        var bindingContext = new InvocationContext(parseResult).BindingContext;

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Default_values_from_options_on_parent_commands_are_bound_by_name_by_default()
    {
        var parentCommand = new Command("parent-command")
        {
            new Option<int>("--int-option", () => 123),
            new Command("child-command")
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        var parseResult = parentCommand.Parse("parent-command child-command");

        var bindingContext = new InvocationContext(parseResult).BindingContext;

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

        var bindingContext = new InvocationContext(parseResult).BindingContext;

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Default_values_from_parent_command_arguments_are_bound_by_name_by_default()
    {
        var parentCommand = new Command("parent-command")
        {
            new Argument<int>(() => 123)
            {
                Name = nameof(ClassWithMultiLetterSetters.IntOption)
            },
            new Command("child-command")
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        var parseResult = parentCommand.Parse("parent-command child-command");

        var bindingContext = new InvocationContext(parseResult).BindingContext;

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void Values_from_options_on_parent_commands_can_be_bound_regardless_of_naming()
    {
        var childCommand = new Command("child-command");
        var option = new Option<int>("-x");
        var parentCommand = new Command("parent-command")
        {
            option,
            childCommand
        };

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        binder.BindMemberFromValue(
            c => c.IntOption,
            option);

        var bindingContext = new InvocationContext(parentCommand.Parse("parent-command -x 123 child-command")).BindingContext;

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

        var bindingContext = new InvocationContext(command.Parse("the-command")).BindingContext;

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(123);
    }

    [Fact]
    public void PropertyInfo_can_be_bound_to_option()
    {
        var command = new Command("the-command");
        var option = new Option<int>("--fred");
        command.Add(option);

        var type = typeof(ClassWithMultiLetterSetters);
        var binder = new ModelBinder(type);
        var propertyInfo = type.GetProperties()[0];

        binder.BindMemberFromValue(
            propertyInfo,
            option);

        var bindingContext = new InvocationContext(command.Parse("the-command --fred 42")).BindingContext;

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

        var bindingContext = new InvocationContext(command.Parse("the-command 42")).BindingContext;

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(42);
    }

    [Fact]
    public void PropertyExpression_can_be_bound_to_option()
    {
        var command = new Command("the-command");
        var option = new Option<int>("--fred");
        command.AddOption(option);

        var binder = new ModelBinder<ClassWithMultiLetterSetters>();

        binder.BindMemberFromValue(
            i => i.IntOption,
            option);

        var bindingContext = new InvocationContext(command.Parse("the-command --fred 42")).BindingContext;

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

        var bindingContext = new InvocationContext(command.Parse("the-command 42")).BindingContext;

        var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

        instance.IntOption.Should().Be(42);
    }

    [Fact]
    public void Option_argument_is_bound_to_longest_constructor()
    {
        var option = new Option<int>("--int-property");

        var bindingContext = new InvocationContext(option.Parse("--int-property 42")).BindingContext;
        var binder = new ModelBinder<ClassWithMultipleCtor>();
        var instance = binder.CreateInstance(bindingContext) as ClassWithMultipleCtor;

        instance.Should().NotBeNull();
        instance.IntProperty.Should().Be(42);
    }

    [Fact]
    public void Command_argument_is_bound_to_longest_constructor()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddArgument(new Argument<int> { Name = nameof(ClassWithMultipleCtor.IntProperty) });
        var parser = new Parser(rootCommand);

        var bindingContext = new InvocationContext(parser.Parse("42")).BindingContext;
        var binder = new ModelBinder<ClassWithMultipleCtor>();
        var instance = binder.CreateInstance(bindingContext) as ClassWithMultipleCtor;

        instance.Should().NotBeNull();
        instance.IntProperty.Should().Be(42);
    }

    [Fact]
    public void Explicit_model_binder_binds_only_to_configured_properties()
    {
        var intOption = new Option<int>("--int-property");
        var stringOption = new Option<string>("--string-property");
        var parser = new Parser(new RootCommand { intOption, stringOption });

        var bindingContext = new InvocationContext(parser.Parse("--int-property 42 --string-property Hello")).BindingContext;
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
        var rootCommand = new RootCommand("Command")
        {
            new Argument<string[]>("names")
        };
        rootCommand.Handler = CommandHandler.Create<string[]>(Handler);
        string[] passedNames = null;
        await rootCommand.InvokeAsync("");

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
        var rootCommand = new RootCommand("Command")
        {
            new Argument<IEnumerable<string>>("names")
        };
        rootCommand.Handler = CommandHandler.Create<IEnumerable<string>>(Handler);
        IEnumerable<string> passedNames = null;
        await rootCommand.InvokeAsync("");

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
        var rootCommand = new RootCommand("Command")
        {
            new Option<string[]>("--names")
        };
        rootCommand.Handler = CommandHandler.Create<string[]>(Handler);
        string[] passedNames = null;
        await rootCommand.InvokeAsync("");

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
        var rootCommand = new RootCommand("Command")
        {
            new Option<IEnumerable<string>>("--names"),
        };
        rootCommand.Handler = CommandHandler.Create<IEnumerable<string>>(Handler);
        IEnumerable<string> passedNames = null;
        await rootCommand.InvokeAsync("");

        passedNames.Should().BeEmpty();

        int Handler(IEnumerable<string> names)
        {
            passedNames = names;
            return 0;
        }
    }

    [Fact]
    public void Custom_ModelBinders_specified_via_BindingContext_can_be_used_for_option_binding()
    {
        ClassWithSetter<int> boundInstance = null;

        var rootCommand = new RootCommand
        {
            new Option<int>("--value")
        };

        rootCommand.Handler = CommandHandler.Create<ClassWithSetter<int>>(x => boundInstance = x);

        var parser = new CommandLineBuilder(rootCommand)
                     .AddMiddleware(context =>
                     {
                         var binder = new ModelBinder<ClassWithSetter<int>>();

                         binder.BindMemberFromValue(instance => instance.Value, _ => 456);

                         context.BindingContext.AddModelBinder(binder);
                     })
                     .Build();

        parser.Invoke("--value 123");

        boundInstance.Value.Should().Be(456);
    }

    [Fact]
    public void Custom_ModelBinders_specified_via_BindingContext_can_be_used_for_command_argument_binding()
    {
        ClassWithSetter<int> boundInstance = null;

        var rootCommand = new RootCommand
        {
            new Argument<int>()
        };

        rootCommand.Handler = CommandHandler.Create<ClassWithSetter<int>>(x => boundInstance = x);

        var parser = new CommandLineBuilder(rootCommand)
                     .AddMiddleware(context =>
                     {
                         var binder = new ModelBinder<ClassWithSetter<int>>();

                         binder.BindMemberFromValue(instance => instance.Value, _ => 456);

                         context.BindingContext.AddModelBinder(binder);
                     })
                     .Build();

        parser.Invoke("123");

        boundInstance.Value.Should().Be(456);
    }

    [Fact]
    public void Default_values_from_options_with_the_same_type_are_bound_and_use_their_own_defaults()
    {
        int first = 0, second = 0;

        var rootCommand = new RootCommand
        {
            new Option<int>("one", () => 1),
            new Option<int>("two", () => 2)
        };
        rootCommand.Handler = CommandHandler.Create<int, int>((one, two) =>
        {
            first = one;
            second = two;
        });

        var parser = new CommandLineBuilder(rootCommand)
            .Build();

        parser.Invoke("");

        first.Should().Be(1);
        second.Should().Be(2);
    }

    [Fact]
    public void Binder_does_not_match_on_partial_name()
    {
        var command = new RootCommand
        {
            new Option<List<string>>("--abc")
        };

        ClassWithOnePropertyNameThatIsSubstringOfAnother boundValue = default;

        command.Handler = CommandHandler.Create(
            (ClassWithOnePropertyNameThatIsSubstringOfAnother s) => { boundValue = s; }
        );

        command.Invoke(new[] { "--abc", "1" });

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

        var cmd = new RootCommand
        {
            Handler = CommandHandler.Create((ClassWithListTypePropertiesAndDefaultCtor value) => { boundInstance = value; })
        };

        var result = cmd.Parse();

        await result.InvokeAsync();

        boundInstance.Should().NotBeNull();
    }

    [Fact]
    public async Task Decimals_are_bound_correctly_when_no_token_is_matched()
    {
        decimal? receivedValue = null;

        var rootCommand = new RootCommand
        {
            new Option<decimal>("--opt-decimal")
        };
        rootCommand.Handler = CommandHandler.Create((ComplexType options) => { receivedValue = options.OptDecimal; });

        await rootCommand.InvokeAsync("");

        receivedValue.Should().Be(0);
    }

    public class ComplexType
    {
        public decimal OptDecimal { get; set; }
    }

    [Fact] // issue: https://github.com/dotnet/command-line-api/issues/1365
    public void Binder_does_not_match_by_substring()
    {
        var rootCommand = new RootCommand
        {
            new Option<string>(
                new[] { "-b", "--bundle" },
                "the path to the app bundle to be installed"),
            new Option<string>(
                new[] { "-1", "--bundle_id", "--bundle-id" },
                "specify bundle id for list and upload")
        };

        DeployOptions boundOptions = null;

        rootCommand.Handler = CommandHandler.Create<DeployOptions>(options =>
        {
            boundOptions = options;
            return 0;
        });

        rootCommand.Invoke("-1 value");

        boundOptions.Bundle.Should().Be(null);
        boundOptions.BundleId.Should().Be("value");
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
        var root = new RootCommand
        {
            new Option<ClassWithSpanConstructor>("--class-with-span-ctor"),
            new Option<int>("--int-value"),
        };

        var handlerWasCalled = false;

        root.Handler = CommandHandler.Create<ClassWithSpanConstructor, int>((spanCtor, intValue) =>
        {
            handlerWasCalled = true;
        });

        root.Invoke(commandLine);

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