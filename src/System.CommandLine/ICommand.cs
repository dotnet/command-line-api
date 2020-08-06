﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public interface ICommand : ISymbol
    {
        bool TreatUnmatchedTokensAsErrors { get; }

        IEnumerable<IArgument> Arguments { get; }
        
        IEnumerable<IOption> Options { get; }
    }
}
