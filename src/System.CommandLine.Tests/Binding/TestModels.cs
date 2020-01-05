// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace System.CommandLine.Tests.Binding
{
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

    public class ClassWithMultiLetterCtorParameters
    {
        public ClassWithMultiLetterCtorParameters(
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

    public class ClassWithCtorParameter<T>
    {
        public ClassWithCtorParameter(T value) => Value = value;

        public T Value { get; }

        public override string ToString() => 
            $"{nameof(ClassWithCtorParameter<T>)}<{typeof(T).Name}>: {Value}";
    }

    public class ClassWithSetter<T>
    {
        public T Value { get; set; }

        public override string ToString() => 
            $"{nameof(ClassWithSetter<T>)}<{typeof(T).Name}>: {Value}";
    }

    public class ClassWithMethodHavingParameter<T>
    {
        private readonly IConsole _console;

        public ClassWithMethodHavingParameter(IConsole console)
        {
            _console = console;
        }

        public int Handle(T value)
        {
            ReceivedValue = value;
            return 0;
        }

        public Task<int> HandleAsync(T value)
        {
            _console.Out.Write(value.ToString());
            return Task.FromResult(Handle(value));
        }

        public T ReceivedValue { get; set; }
    }

    public class TypeWithInvokeAndCtor
    {
        public TypeWithInvokeAndCtor(int intFromCtor, string stringFromCtor)
        {
            IntValueFromCtor = intFromCtor;
            StringValueFromCtor = stringFromCtor;
        }

        public int IntValueFromCtor { get; }

        public string StringValueFromCtor { get; }

        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Task<int> Invoke(string stringParam, int intParam)
        {
            return Task.FromResult(76);
        }
    }

    public class ClassWithInvokeAndDefaultCtor
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Task<int> Invoke(string stringParam, int intParam)
        {
            return Task.FromResult(66);
        }

        public Task<int> SomethingElse(int intParam, string stringParam)
        {
            return Task.FromResult(67);
        }
    }

    public class ClassWithStaticsInvokeAndCtor
    {
        public ClassWithStaticsInvokeAndCtor(int intFromCtor, string stringFromCtor)
        {
            IntValueFromCtor = intFromCtor;
            StringValueFromCtor = stringFromCtor;
        }

        public static int StaticIntProperty { get; set; } = 67;

        public static string StaticStringProperty { get; set; }

        public int IntValueFromCtor { get; }

        public string StringValueFromCtor { get; }

        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public static Task<int> Invoke(string stringParam, int intParam)
        {
            return Task.FromResult(96);
        }
    }

    public class ClassWithParameterlessInvokeAndDefaultCtor
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Task<int> Invoke()
        {
            return Task.FromResult(86);
        }
    }

    public class ClassWithMultipleCtor
    {
        public ClassWithMultipleCtor()
        {

        }

        public ClassWithMultipleCtor(int intProperty)
        {
            IntProperty = intProperty;
        }

        public int IntProperty { get; }
    }
}
