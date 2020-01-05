// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    public enum TokenType
    {
        Argument,
        Command,
        Option,
        EndOfArguments,
        Operand,
        Directive
    }
}
