// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;

namespace System.CommandLine.Rendering.Tests
{
    public class SystemConsoleTerminalTests : TerminalTests
    {
        protected override ITerminal GetTerminal()
        {
            return new SystemConsoleTerminal(new SystemConsole());
        }
    }
}
