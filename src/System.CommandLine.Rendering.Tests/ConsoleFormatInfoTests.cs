// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Tests.Utility;
using System.Globalization;
using Xunit;

namespace System.CommandLine.Rendering.Tests
{
    public class ConsoleFormatInfoTests
    {
        [Fact]
        public void Can_create_modify_and_readonly_format_info()
        {
            var info = new ConsoleFormatInfo();
            info.IsReadOnly
                .Should()
                .BeFalse();

            info.SupportsAnsiCodes = true;
            info.SupportsAnsiCodes
                .Should()
                .BeTrue();

            var readonlyInfo = ConsoleFormatInfo.ReadOnly(info);
            readonlyInfo.IsReadOnly
                .Should()
                .BeTrue();

            Assert.Throws<InvalidOperationException>(() => readonlyInfo.SupportsAnsiCodes = false);
        }

        [Fact]
        public void ReadOnly_throws_argnull()
        {
            Assert.Throws<ArgumentNullException>(() => ConsoleFormatInfo.ReadOnly(null));
        }

        [Fact]
        public void Set_current_throws_argnull()
        {
            var info = new ConsoleFormatInfo();
            Assert.Throws<ArgumentNullException>(() => ConsoleFormatInfo.CurrentInfo = null);
        }

        [Fact]
        public void GetInstance_null_returns_current()
        {
            var info = ConsoleFormatInfo.GetInstance(null);
            info.Should()
                .BeSameAs(ConsoleFormatInfo.CurrentInfo);
        }

        [Fact]
        public void GetInstance_returns_same()
        {
            var info = new ConsoleFormatInfo();

            var instance = ConsoleFormatInfo.GetInstance(info);
            instance.Should()
                .BeSameAs(info);
            instance.Should()
                .NotBeSameAs(ConsoleFormatInfo.CurrentInfo);
        }

        [Fact]
        public void GetInstance_calls_GetFormat_on_provider()
        {
            var info = new ConsoleFormatInfo();
            var provider = new MockFormatProvider() { TestInfo = info };

            var instance = ConsoleFormatInfo.GetInstance(provider);
            instance.Should()
                .BeSameAs(info);
            instance.Should()
                .NotBeSameAs(ConsoleFormatInfo.CurrentInfo);

            provider.GetFormatCallCount
                .Should()
                .Be(1);
        }

        private class MockFormatProvider : IFormatProvider
        {
            public int GetFormatCallCount { get; set; }
            public ConsoleFormatInfo TestInfo { get; set; }
            public object GetFormat(Type formatType)
            {
                GetFormatCallCount++;

                if (formatType == typeof(ConsoleFormatInfo))
                {
                    return TestInfo;
                }

                throw new NotSupportedException();
            }
        }

        [Fact]
        public void GetFormat_returns_instance()
        {
            var info = new ConsoleFormatInfo();
            info.GetFormat(typeof(ConsoleFormatInfo))
                .Should()
                .BeSameAs(info);
        }

        [Fact]
        public void GetFormat_returns_null()
        {
            var info = new ConsoleFormatInfo();
            info.GetFormat(typeof(NumberFormatInfo))
                .Should()
                .BeNull();
        }
    }
}
