// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class EntryPointCreatorTests
    {
        [Fact]
        public void ItThrowsIfEntryPointNotFound()
        {
            Action find = () => EntryPointDiscoverer.FindStaticEntryMethod(typeof(IEnumerable<>).Assembly);
            find.Should().Throw<InvalidProgramException>();
        }

        private class Program
        {
            public static void Main(string arg1) { }
            public static void Main(string arg2, string arg3) { }
        }

        [Fact]
        public void ItThrowsIfMultipleEntryPointNotFound()
        {
            Action find = () => EntryPointDiscoverer.FindStaticEntryMethod(typeof(CommandLineTests).Assembly);
            find.Should().Throw<AmbiguousMatchException>();
        }
    }
}
