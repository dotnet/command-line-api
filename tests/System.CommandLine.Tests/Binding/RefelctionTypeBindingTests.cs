﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Binding;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.CommandLine.Builder;

namespace System.CommandLine.Tests.Binding
{
    public class ReflectionTypeBindingTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task Can_invoke_default_method_on_type_with_parameters()
        {
            const string commandLine = "command --intParam 425 --stringParam Gandalf";
            const int result = 66;
            object[] expectedArgumets = { "Gandalf", 425 };

            var command = new Command("command");
            command.AddOption(
                    new Option("--stringParam",
                               argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intParam",
                   argument: new Argument<int>()));

            command.Handler = CommandHandler.Create<TypeWithInvokeNoCtor>();
            var binder = (command.Handler as ReflectionCommandHandler).Binder;

            var invocationContext = command.MakeDefaultInvocationContext(commandLine);
            var arguments = binder.GetInvocationArguments(invocationContext);
            var target = binder.GetTarget(invocationContext);
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);
            target.Should().BeOfType<TypeWithInvokeNoCtor>();
            var typedTarget = target as TypeWithInvokeNoCtor;
            typedTarget.IntProperty.Should().Be(default);
            typedTarget.StringProperty.Should().Be(default);

            var ret = await command.InvokeAsync(commandLine, _console);

            ret.Should().Be(result);
        }

        [Fact]
        public async Task Can_invoke_named_method_on_type_with_parameters()
        {
            const string commandLine = "command --intParam 425 --stringParam Gandalf";
            const int result = 67;
            object[] expectedArgumets = { 425, "Gandalf" };

            var command = new Command("command");
            command.AddOption(
                    new Option("--stringParam",
                               argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intParam",
                   argument: new Argument<int>()));

            command.Handler = CommandHandler.Create<TypeWithInvokeNoCtor>(nameof(TypeWithInvokeNoCtor.SomethingElse));
            var binder = (command.Handler as ReflectionCommandHandler).Binder;

            var invocationContext = command.MakeDefaultInvocationContext(commandLine);
            var arguments = binder.GetInvocationArguments(invocationContext);
            var target = binder.GetTarget(invocationContext);
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);
            target.Should().NotBeNull();
            target.Should().BeOfType<TypeWithInvokeNoCtor>();
            var typedTarget = target as TypeWithInvokeNoCtor;
            typedTarget.IntProperty.Should().Be(default);
            typedTarget.StringProperty.Should().Be(default);

            var ret = await command.InvokeAsync(commandLine, _console);

            ret.Should().Be(result);
        }

        [Fact]
        public async Task Properties_are_set_before_method_is_invoked()
        {
            const string commandLine = "command --intParam 425 --stringParam Gandalf --intProperty 111 --stringProperty Bilbo";
            const int result = 66;
            object[] expectedArgumets = { "Gandalf", 425 };

            var command = new Command("command");
            command.AddOption(
                    new Option("--stringParam",
                    argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intParam",
                   argument: new Argument<int>()));
            command.AddOption(
                   new Option("--stringProperty",
                   argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intProperty",
                   argument: new Argument<int>()));

            command.Handler = CommandHandler.Create<TypeWithInvokeNoCtor>();
            var binder = (command.Handler as ReflectionCommandHandler).Binder;

            var invocationContext = command.MakeDefaultInvocationContext(commandLine);
            var arguments = binder.GetInvocationArguments(invocationContext);
            var target = binder.GetTarget(invocationContext);
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            target.Should().NotBeNull();
            target.Should().BeOfType<TypeWithInvokeNoCtor>();
            var typedTarget = target as TypeWithInvokeNoCtor;
            typedTarget.IntProperty.Should().Be(111);
            typedTarget.StringProperty.Should().Be("Bilbo");

            var ret = await command.InvokeAsync(commandLine, _console);

            ret.Should().Be(result);
        }

        [Fact]
        public async Task Parameterless_ctor_and_invoke_provides_properties()
        {
            const string commandLine = "command --intProperty 111 --stringProperty Bilbo";
            const int result = 86;
            object[] expectedArgumets = { };

            var command = new Command("command");
            command.AddOption(
                   new Option("--stringProperty",
                   argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intProperty",
                   argument: new Argument<int>()));

            command.Handler = CommandHandler.Create<TypeWithParameterlessInvokeAndCtor>();
            var binder = (command.Handler as ReflectionCommandHandler).Binder;

            var invocationContext = command.MakeDefaultInvocationContext(commandLine);
            var arguments = binder.GetInvocationArguments(invocationContext);
            var target = binder.GetTarget(invocationContext);
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            target.Should().NotBeNull();
            target.Should().BeOfType<TypeWithParameterlessInvokeAndCtor>();
            var typedTarget = target as TypeWithParameterlessInvokeAndCtor;
            typedTarget.IntProperty.Should().Be(111);
            typedTarget.StringProperty.Should().Be("Bilbo");

            var ret = await command.InvokeAsync(commandLine, _console);

            ret.Should().Be(result);
        }


        [Fact]
        public async Task Parameterized_constructors_parameters_will_be_used()
        {
            const string commandLine = "command --intParam 425 --stringParam Gandalf --intFromCtor 50 --stringFromCtor Frodo --intProperty 111 --stringProperty Bilbo";
            const int result = 76;
            object[] expectedArgumets = { "Gandalf", 425 };

            var command = new Command("command");
            command.AddOption(
                    new Option("--stringParam",
                    argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intParam",
                   argument: new Argument<int>()));
            command.AddOption(
                   new Option("--stringProperty",
                   argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intProperty",
                   argument: new Argument<int>()));
            command.AddOption(
                    new Option("--stringFromCtor",
                    argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intFromCtor",
                   argument: new Argument<int>()));

            command.Handler = CommandHandler.Create<TypeWithInvokeAndCtor>();
            var binder = (command.Handler as ReflectionCommandHandler).Binder;

            var invocationContext = command.MakeDefaultInvocationContext(commandLine);
            var arguments = binder.GetInvocationArguments(invocationContext);
            var target = binder.GetTarget(invocationContext);
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            target.Should().NotBeNull();
            target.Should().BeOfType<TypeWithInvokeAndCtor>();
            var typedTarget = target as TypeWithInvokeAndCtor;
            typedTarget.IntProperty.Should().Be(111);
            typedTarget.StringProperty.Should().Be("Bilbo");
            typedTarget.IntValueFromCtor.Should().Be(50);
            typedTarget.StringValueFromCtor.Should().Be("Frodo");

            var ret = await command.InvokeAsync(commandLine, _console);

            ret.Should().Be(result);
        }

        [Fact]
        public async Task Static_properties_instance_properties_ctor_and_instance_params_can_be_used_together()
        {
            const string commandLine = "command --intParam 425 --stringParam Gandalf --intFromCtor 50 --staticIntProperty 142 --stringFromCtor Frodo --intProperty 111 --staticStringProperty Gimli --stringProperty Bilbo";
            const int result = 96;
            object[] expectedArgumets = { "Gandalf", 425 };

            var command = new Command("command");
            command.AddOption(
                    new Option("--stringParam",
                    argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intParam",
                   argument: new Argument<int>()));
            command.AddOption(
                   new Option("--stringProperty",
                   argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intProperty",
                   argument: new Argument<int>()));
            command.AddOption(
                    new Option("--stringFromCtor",
                    argument: new Argument<string>()));
            command.AddOption(
                   new Option("--intFromCtor",
                   argument: new Argument<int>()));
            command.AddOption(
                    new Option("--staticStringProperty",
                    argument: new Argument<string>()));
            command.AddOption(
                   new Option("--staticIntProperty",
                   argument: new Argument<int>()));

            command.Handler = CommandHandler.Create<TypeWithStaticsInvokeAndCtor>();
            var binder = (command.Handler as ReflectionCommandHandler).Binder;

            var invocationContext = command.MakeDefaultInvocationContext(commandLine);
            var arguments = binder.GetInvocationArguments(invocationContext);
            var target = binder.GetTarget(invocationContext);
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            target.Should().NotBeNull();
            target.Should().BeOfType<TypeWithStaticsInvokeAndCtor>();
            var typedTarget = target as TypeWithStaticsInvokeAndCtor;
            typedTarget.IntProperty.Should().Be(111);
            typedTarget.StringProperty.Should().Be("Bilbo");
            typedTarget.IntValueFromCtor.Should().Be(50);
            typedTarget.StringValueFromCtor.Should().Be("Frodo");
            TypeWithStaticsInvokeAndCtor.StaticIntProperty.Should().Be(142);
            TypeWithStaticsInvokeAndCtor.StaticStringProperty.Should().Be("Gimli");

            var ret = await command.InvokeAsync(commandLine, _console);

            ret.Should().Be(result);
        }
    }
}
