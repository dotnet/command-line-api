// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.IO;

namespace System.CommandLine.Tests.Help
{
    public static class HelpBuilderExtensions
    {
        public static void Write(
            this HelpBuilder builder,
            CliCommand command,
            TextWriter writer) =>
            builder.Write(new HelpContext(builder, command, writer));
    }
}