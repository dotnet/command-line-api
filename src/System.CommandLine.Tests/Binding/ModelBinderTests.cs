using System.CommandLine.Binding;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class ModelBinderTests
    {
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
            var descriptor = ModelDescriptor.FromType<ClassWithMultiLetterSetters>();
            var bindingContext = new BindingContext(parser.Parse("the-command --int-option 123"));
            var binder = new ModelBinder(descriptor);

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
            var descriptor = ModelDescriptor.FromType<ClassWithMultiLetterSetters>();
            var instance = new ClassWithMultiLetterSetters();
            var bindingContext = new BindingContext(parser.Parse("the-command --int-option 123"));
            var binder = new ModelBinder(descriptor);

            binder.UpdateInstance(instance, bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact(Skip = "wip")]
        public void Option_to_property_naming_conventions_can_be_specified_using_the_binder()
        {
            // TODO-JOSEQU (Option_to_property_naming_conventions_can_be_specified_using_the_binder) write test
            Assert.True(false, "Test Option_to_property_naming_conventions_can_be_specified_using_the_binder is not written yet.");
        }

        [Fact(Skip = "wip")]
        public void Values_from_parent_commands_can_be_bound()
        {
            var childCommand = new Command("child-command");
            var option = new Option("--int-option")
                         {
                             Argument = new Argument<int>()
                         };
            var parentCommand = new Command("parent-command")
                                {
                                    option,
                                    childCommand
                                };
            var parser = new Parser(parentCommand);

            var binder = ModelDescriptor.FromType<ClassWithMultiLetterSetters>()
                                        .CreateBinder();

            var bindingContext = new BindingContext(
                parser.Parse("parent-command --int-option 123 child-command"));

            binder.BindProperty(
                c => c.IntOption,
                option);

            var instance = (ClassWithMultiLetterSetters)binder.CreateInstance(bindingContext);

            instance.IntOption.Should().Be(123);
        }

        [Fact(Skip = "wip")]
        public void Environment_variables_can_be_value_providers()
        {
            // TODO (Environment_variables_can_be_value_providers) write test
            Assert.True(false, "Test Environment_variables_can_be_value_providers is not written yet.");
        }

        [Fact(Skip = "wip")]
        public void Parser_can_be_created_based_on_target_types()
        {
            // var parser = CreateParserFrom<>();

            // TODO (Parser_can_be_created_based_on_target_types) write test
            Assert.True(false, "Test Parser_can_be_created_based_on_target_types is not written yet.");
        }
    }
}
