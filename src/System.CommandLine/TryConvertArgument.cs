// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public delegate bool TryConvertArgument(SymbolResult symbolResult, out object value);

    public delegate bool TryConvertArgument<T>(SymbolResult symbolResult, out T value);
}
