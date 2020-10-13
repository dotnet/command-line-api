// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine
{
    public interface IOption : IIdentifierSymbol, IValueDescriptor
    {
        IArgument Argument { get; }

        bool IsRequired { get; }
    }
}
