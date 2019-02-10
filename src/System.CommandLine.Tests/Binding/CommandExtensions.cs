// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Builder;

namespace System.CommandLine.Tests.Binding
{
    public static class CommandExtensions
    {
        public static BindingContext CreateBindingContext(
            this Command command,
            string commandLine)
        {
            var parser = new CommandLineBuilder(command)
                         .UseDefaults()
                         .Build();

            var parseResult = parser.Parse(commandLine);

            return new BindingContext(parseResult);
        }
    }
}
