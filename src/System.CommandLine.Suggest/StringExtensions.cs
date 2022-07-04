﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Suggest
{
    internal static class StringExtensions
    {
        public static string Escape(this string commandLine) => commandLine.Replace("\"", "\\\"");
    }
}
