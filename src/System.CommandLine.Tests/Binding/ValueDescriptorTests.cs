// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class ValueDescriptorTests
    {
        [Fact]
        public void Custom_value_descriptor_can_bind_complex_type()
        {
            // TODO (testname) write test
            throw new NotImplementedException();
        }

        [Fact]
        public void Value_Descriptor_can_bind_well_known_injected_types()
        {
            var optionX = new Option<string>("-x");
            var optionY = new Option<int>("-y");
            var command = new RootCommand
            {
                optionX,
                optionY
            };

            string boundX = null;
            int boundY = 0;
            ParseResult boundParseResult = null;

            // command.SetHandler(
            //     optionX,
            //     optionY,
            //     (x, y, parseResult) =>
            //     {
            //         boundX = x;
            //         boundY = y;
            //         boundParseResult = parseResult;
            //     });

            command.Invoke("-x hello -y 123");

            boundX.Should().Be("hello");
            boundY.Should().Be(123);
            boundParseResult.Should().NotBeNull();
        }

        [Fact]
        public void Value_Descriptor_can_bind_user_defined_injected_types()
        {
            // TODO (Value_Descriptor_can_bind_well_known_injected_types) write test
            throw new NotImplementedException();
        }
    }

    public class Injected<T> : IValueDescriptor<T>
    {
        private string _valueName;
        private Type _valueType;
        private bool _hasDefaultValue;

        public string ValueName => _valueName;

        public Type ValueType => _valueType;

        public bool HasDefaultValue => _hasDefaultValue;

        public object? GetDefaultValue()
        {
            return null;
        }
    }
}