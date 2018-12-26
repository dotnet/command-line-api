// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    internal class DefaultConsoleFactory : IConsoleFactory
    {
        public IConsole CreateConsole()
        {
            if (Console.IsOutputRedirected)
            {
                return new SystemConsole();
            }
            else
            {
                return new SystemTerminal();
            }
        }
    }
}
