// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public interface IArgument
    {
        IArgumentArity Arity { get; }

        bool HasDefaultValue { get; }

        object GetDefaultValue();

        string Name { get; }

        string Description { get; }

        bool IsHidden { get; }
    }
}
