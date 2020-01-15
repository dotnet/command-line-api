// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public enum MiddlewareOrder : byte
    {
        ProcessStart = byte.MinValue,
        ExceptionHandler = ProcessStart + 10,
        Configuration = ExceptionHandler + 10,
        Default = Configuration + 50,
        ErrorReporting = Default + 10,
    }
}