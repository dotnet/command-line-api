// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    internal static class StringExtensions
    {
        public static bool EndsWithWhitespace(this string value) =>
            value.Length > 0
            && char.IsWhiteSpace(value[value.Length - 1]);

        public static bool StartsWithWhitespace(this string value) =>
            value.Length > 0
            && char.IsWhiteSpace(value[0]);

        public static bool IsNewLine(this string value) => value == "\n" || value == "\r\n";
    }
}
