// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public static class ArgumentArity
    {
        public static ArgumentArityValidator Zero { get; } = new ArgumentArityValidator(0, 0);

        public static ArgumentArityValidator ZeroOrOne { get; } = new ArgumentArityValidator(0, 1);

        public static ArgumentArityValidator ExactlyOne { get; } = new ArgumentArityValidator(1, 1);

        public static ArgumentArityValidator ZeroOrMore { get; } = new ArgumentArityValidator(0, int.MaxValue);

        public static ArgumentArityValidator OneOrMore { get; } = new ArgumentArityValidator(1, int.MaxValue);
    }
}
