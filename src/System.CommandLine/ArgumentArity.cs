// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public static class ArgumentArity
    {
        public static ArgumentArityValidator Zero { get; } = new ArgumentArityValidator(0, 0);
        public static ArgumentArityValidator One { get; } = new ArgumentArityValidator(0, 1);
        public static ArgumentArityValidator Many { get; } = new ArgumentArityValidator(0, int.MaxValue);
    }
}
