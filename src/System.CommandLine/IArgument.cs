﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine
{
    public interface IArgument : 
        ISymbol,
        ISuggestionSource, 
        IValueDescriptor
    {
        IArgumentArity Arity { get; }
    }
}
