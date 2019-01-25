// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using System.CommandLine.Invocation;
using System.CommandLine.JackFruit.Tests.MethodModel;
using System.CommandLine.Builder;
using System.CommandLine.Binding;

namespace System.CommandLine.JackFruit.Tests
{
    public class ReflectionCommandHandlerTests
    {
        private readonly TestConsole _console;
        private readonly TestProgram _testProgram;
        private readonly Command testParent;


        public ReflectionCommandHandlerTests()
        {
            _console = new TestConsole();
            _testProgram = new TestProgram();
            var helpFinder = PreBinderContext.Current.DescriptionStrategies;
            helpFinder.AddStrategy<object>((c, s) => DescriptionFinder.Description(s));
            helpFinder.AddStrategy<object>((c, s) => HybridModelDescriptionFinder.Description(s));
            testParent = new Command("test");
        }

        [Fact]
        public void Can_add_and_use_parameterInfo_valueFunc_bindings()
        {
            var type = typeof(Fruit);
            var methodInfo = type.GetMethod(nameof(Fruit.Bowl));
            var handler = ReflectionCommandHandler.Create(methodInfo);
            handler.Should().NotBeNull();
            var command = new RootCommand();
            command.Handler = handler;
            handler.Binder.AddBinding(methodInfo.GetParameters()[0], ValueBindingSide.Create(() => "Water"));
            handler.Binder.AddBinding(methodInfo.GetParameters()[1], ValueBindingSide.Create(() => true));
            handler.Binder.AddBinding(methodInfo.GetParameters()[2], ValueBindingSide.Create(() => 1_000_000));
            handler.Binder.AddBinding(methodInfo.GetParameters()[3], ValueBindingSide.Create(() => "Cavendish"));

            const string commandLine = "";
            var arguments = ((command.Handler as IBoundCommandHandler).Binder as ReflectionBinder)
                            .GetInvocationArguments(GetInvocationContext(commandLine, command));
            arguments.Should().BeEquivalentTo("Water", true, 1_000_000, "Cavendish");
        }

        [Fact]
        public void Can_add_and_use_parameterInfo_option_bindings()
        {
            var type = typeof(Fruit);
            var methodInfo = type.GetMethod(nameof(Fruit.Bowl));
            var handler = ReflectionCommandHandler.Create(methodInfo);
            handler.Should().NotBeNull();

            var command = new RootCommand();
            command.Handler = handler;
            var melonOption = new Option("melon", "", new Argument<string>());
            var berryOption = new Option("berry", "", new Argument<bool>());
            var mangoOption = new Option("mango", "", new Argument<int>());
            var bananaOption = new Option("banana", "", new Argument<string>());
            handler.Binder.AddBinding(methodInfo.GetParameters()[0], SymbolBindingSide.Create(melonOption));
            handler.Binder.AddBinding(methodInfo.GetParameters()[1], SymbolBindingSide.Create(berryOption));
            handler.Binder.AddBinding(methodInfo.GetParameters()[2], SymbolBindingSide.Create(mangoOption));
            handler.Binder.AddBinding(methodInfo.GetParameters()[3], SymbolBindingSide.Create(bananaOption));
            command.AddOptions(new Option[] { melonOption, berryOption, mangoOption, bananaOption });

            const string commandLine = "melon Cantalope berry true mango 62 banana ohYeah";
            var arguments = ((command.Handler as IBoundCommandHandler).Binder as ReflectionBinder)
                            .GetInvocationArguments(GetInvocationContext(commandLine, command));
            arguments.Should().BeEquivalentTo("Cantalope", true, 62, "ohYeah");
        }

        [Fact]
        public void Can_add_and_use_propertyInfo_valueFunc_bindings()
        {
            var type = typeof(FruitType);
            var handler = ReflectionCommandHandler.Create(type);
            handler.Should().NotBeNull();
            handler.Binder.AddBinding(type.GetProperties()[0], ValueBindingSide.Create(() => "Water"));
            handler.Binder.AddBinding(type.GetProperties()[1], ValueBindingSide.Create(() => true));
            handler.Binder.AddBinding(type.GetProperties()[2], ValueBindingSide.Create(() => 1_000_000));
            handler.Binder.AddBinding(type.GetProperties()[3], ValueBindingSide.Create(() => "Cavendish"));
            var target = handler.Binder.GetTarget();
            target.Should().BeOfType<FruitType>();
            var fruit = target as FruitType;
            fruit.Melon.Should().Be("Water");
            fruit.Berry.Should().BeTrue();
            fruit.Mango.Should().Be(1_000_000);
            fruit.Banana.Should().Be("Cavendish");
        }

        [Fact]
        public void Can_add_and_use_propertyInfo_option_bindings()
        {
            var type = typeof(FruitType);
            var handler = ReflectionCommandHandler.Create(type);
            handler.Should().NotBeNull();

            var command = new RootCommand();
            command.Handler = handler;
            var melonOption = new Option("melon", "", new Argument<string>());
            var berryOption = new Option("berry", "", new Argument<bool>());
            var mangoOption = new Option("mango", "", new Argument<int>());
            var bananaOption = new Option("banana", "", new Argument<string>());
            handler.Binder.AddBinding(type.GetProperties()[0], SymbolBindingSide.Create( melonOption));
            handler.Binder.AddBinding(type.GetProperties()[1], SymbolBindingSide.Create( berryOption));
            handler.Binder.AddBinding(type.GetProperties()[2], SymbolBindingSide.Create( mangoOption));
            handler.Binder.AddBinding(type.GetProperties()[3], SymbolBindingSide.Create(bananaOption));
            command.AddOptions(new Option[] { melonOption, berryOption, mangoOption, bananaOption });

            var commandLine ="melon Cantalope berry true mango 62 banana ohYeah";
            var target = ((command.Handler as IBoundCommandHandler).Binder as ReflectionBinder)
                            .GetTarget (GetInvocationContext(commandLine, command))
                            as FruitType;
            target.Should().NotBeNull();
            target.Melon.Should().Be("Cantalope");
            target.Berry.Should().Be(true);
            target.Mango.Should().Be(62);
            target.Banana.Should().Be("ohYeah");
        }

        private static InvocationContext GetInvocationContext(string commandLine, Command command)
        {
            var parser = new CommandLineBuilder(command)
                         .UseDefaults()
                         .Build();
            var parseResult = parser.Parse(commandLine);
            var invocationContext = new InvocationContext(parseResult, parser);
            return invocationContext;
        }
    } 
}
