// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Reflection;
using System.CommandLine.JackFruit.Tests.MethodModel;
using System.CommandLine.Builder;

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
            handler.Binder.AddBinding(methodInfo.GetParameters()[0], ValueBindingSide.Create(() => "Water"));
            handler.Binder.AddBinding(methodInfo.GetParameters()[1], ValueBindingSide.Create(() => true));
            handler.Binder.AddBinding(methodInfo.GetParameters()[2], ValueBindingSide.Create(() => 1_000_000));
            handler.Binder.AddBinding(methodInfo.GetParameters()[3], ValueBindingSide.Create(() => "Cavendish"));

            // TODO: Figure out how to assert
            //CheckFuncBindAction<Fruit>(handler.Binder, methodInfo.GetParameters()[0], typeof(string));
            //CheckFuncBindAction<Fruit>(handler.Binder, methodInfo.GetParameters()[1], typeof(bool));
            //CheckFuncBindAction<Fruit>(handler.Binder, methodInfo.GetParameters()[2], typeof(int));
            //CheckFuncBindAction<Fruit>(handler.Binder, methodInfo.GetParameters()[3], typeof(string));
            var task = handler.InvokeAsync(null);
            task.Should().NotBeNull();
            Fruit.Captured.Should().Be(@"Melon = Water
Berry = True
Mango = 1000000
Banana = Cavendish");
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

            // TODO: Figure out what internals you want to be able to test 
            //CheckOptionAction(handler.Binder, methodInfo.GetParameters()[0].Name, methodInfo.GetParameters()[0], typeof(string));
            //CheckOptionAction(handler.Binder, methodInfo.GetParameters()[1].Name, methodInfo.GetParameters()[1], typeof(bool));
            //CheckOptionAction(handler.Binder, methodInfo.GetParameters()[2].Name, methodInfo.GetParameters()[2], typeof(int));
            //CheckOptionAction(handler.Binder, methodInfo.GetParameters()[3].Name, methodInfo.GetParameters()[3], typeof(string));

            var parser = new CommandLineBuilder(command).Build();
            var parseResult = parser.Parse("melon Cantalope berry true mango 62 banana ohYeah");
            var task = parser.InvokeAsync(parseResult);

            task.Should().NotBeNull();
            const string expected = @"Melon = Cantalope
Berry = True
Mango = 62
Banana = ohYeah";
            Fruit.Captured.Should().Be(expected);
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

            // TODO: Figure out how to assert
            //CheckOptionAction(handler.Binder, type.GetProperties()[0].Name, type.GetProperties()[0], typeof(string));
            //CheckOptionAction(handler.Binder, type.GetProperties()[1].Name, type.GetProperties()[1], typeof(bool));
            //CheckOptionAction(handler.Binder, type.GetProperties()[2].Name, type.GetProperties()[2], typeof(int));
            //CheckOptionAction(handler.Binder, type.GetProperties()[3].Name, type.GetProperties()[3], typeof(string));

            var parser = new CommandLineBuilder(command).Build();
            var parseResult = parser.Parse("melon Cantalope berry true mango 62 banana ohYeah");
            var task = parser.InvokeAsync(parseResult);

            task.Should().NotBeNull();
            const string expected = @"Melon = Cantalope
Berry = True
Mango = 62
Banana = ohYeah";
            Fruit.Captured.Should().Be(expected);
        }

        //private void CheckArgumentAction(SymbolBindingSide bindAction,
        //         ParameterInfo parameterInfo, Type returnType)
        //{
        //    bindAction.Should().NotBeNull();
        //    bindAction.ReflectionThing.Should().Be(parameterInfo);
        //    bindAction.Symbol.Should().NotBeNull();
        //    var argument = bindAction.Symbol as Argument;
        //    argument.Should().NotBeNull();
        //    argument.Name.Should().BeEquivalentTo(parameterInfo.Name);
        //    argument.ArgumentType.Should().Be(parameterInfo.ParameterType);
        //    bindAction.ReturnType.Should().Be(returnType);
        //}

        //private void CheckOptionAction(ReflectionBinder binder,
        //        string name, object reflectionThing, Type returnType)
        //{
        //    var bindAction = binder.Find(reflectionThing);
        //    bindAction.Should().NotBeNull();
        //    var optionBindAction = bindAction as SymbolBindingSide;
        //    optionBindAction.Should().NotBeNull();
        //    optionBindAction.ReflectionThing.Should().Be(reflectionThing);
        //    optionBindAction.Symbol.Should().NotBeNull();
        //    var option = optionBindAction.Symbol as Option;
        //    option.Should().NotBeNull();
        //    option.Name.Should().BeEquivalentTo(name);
        //    if (returnType == typeof(bool))
        //    {
        //        // backwards but effective
        //        new Type[] { null, typeof(bool) }.Should().Contain(option.Argument.ArgumentType);
        //    }
        //    else
        //    {
        //        option.Argument.ArgumentType.Should().Be(returnType);
        //    }
        //    bindAction.ReturnType.Should().Be(returnType);
        //}

        //private void CheckFuncBindAction<T>(ReflectionBinder binder,
        //        object reflectionThing, Type returnType)
        //    where T : class
        //{
        //    var bindAction = binder.Find(reflectionThing);
        //    bindAction.Should().NotBeNull();
        //    var funcBindAction = bindAction as FuncBinding<T>;
        //    funcBindAction.Should().NotBeNull();
        //    funcBindAction.ReflectionThing.Should().Be(reflectionThing);
        //    funcBindAction.ValueFunc.Should().NotBeNull();
        //    funcBindAction.ReturnType.Should().Be(returnType);

        //}
    }
}
