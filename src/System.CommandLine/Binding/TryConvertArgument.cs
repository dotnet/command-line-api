// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Binding
{
    public delegate bool TryConvertArgument(
        ArgumentResult symbolResult,
        out object value);

    public delegate bool TryConvertArgument<T>(
        ArgumentResult symbolResult,
        out T value);
}