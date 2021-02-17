// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions.Primitives;

namespace System.CommandLine.Tests.Utility
{
    public class ConsoleAssertions : ReferenceTypeAssertions<IConsole, ConsoleAssertions>
    {
        public ConsoleAssertions(IConsole console)
        {
            Subject = console;
        }

        protected override string Identifier => nameof(IConsole);
    }
}