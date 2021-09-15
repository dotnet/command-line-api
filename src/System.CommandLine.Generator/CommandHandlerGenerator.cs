﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public sealed class CommandHandlerGenerator
    {
        private CommandHandlerGenerator()
        {
        }

        public static CommandHandlerGenerator GeneratedHandler { get; } = null!;
    }
}