// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public interface IArgument
    {
        IHelpDetail Help { get; }

        ArgumentArity Arity { get; }

        bool HasDefaultValue { get; }

        object GetDefaultValue();
    }
}
