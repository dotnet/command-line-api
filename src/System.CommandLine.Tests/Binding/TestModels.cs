// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace System.CommandLine.Tests.Binding
{
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

    public class ClassWithComplexTypes
    {
        public int IntOption { get; set; }
        public string StringOption { get; set; }
        public bool BoolOption { get; set; }
        public List<ClassWithMultiLetterSetters> ListOptionDefaultNull { get; set; }
        public List<ClassWithCtorParameter<string>> ListOptionDefaultEmpty { get; set; } = new ();
    }

    public class ClassWithListTypePropertiesAndDefaultCtor
    {
        public List<string> Strings { get; set; }

        public IEnumerable<int> BoolOption { get; set; }
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
        private readonly TextWriter _output;

        public ClassWithMethodHavingParameter(ParseResult parseResult)
        {
            _output = parseResult.Configuration.Output;
        }

        public int Handle(T value)
        {
            ReceivedValue = value;
            return 0;
        }

        public Task<int> HandleAsync(T value)
        {
            _output.Write(value.ToString());
            return Task.FromResult(Handle(value));
        }

        public T ReceivedValue { get; set; }
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

    public class ClassWithOnePropertyNameThatIsSubstringOfAnother
    {
        public List<string> Abc { get; set; }

        public string AbcDef { get; set; }
    }
}
