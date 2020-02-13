// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandExtensionsTests
    {
        [Fact]
        public void Mulitple_invocations_via_Invoke_extension_will_not_reconfigure_implicit_parser()
        {
            var command = new RootCommand("Root command description")
            {
                new Command("inner")
            };

            var console1 = new TestConsole();

            command.Invoke("-h", console1);

            console1.Out.ToString().Should().Contain(command.Description);

            var console2 = new TestConsole();

            command.Invoke("-h", console2);

            console2.Out.ToString().Should().Contain(command.Description);
        }
    }
}