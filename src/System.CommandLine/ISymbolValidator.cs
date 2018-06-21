// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public interface ISymbolValidator
    {
        string Validate(Symbol symbol);
    }
}
