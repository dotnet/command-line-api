// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.IO
{
    public interface IStandardError
    {
        IStandardStreamWriter Error { get; }

        bool IsErrorRedirected { get; }
    }
}