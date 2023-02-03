// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Designates ordering of middleware in the invocation pipeline.
    /// </summary>
    public enum MiddlewareOrder
    {
        /// <summary>
        /// The position in the pipeline at which the exception handler middleware is invoked.
        /// </summary>
        ExceptionHandler = -2000,

        /// <summary>
        /// The position in the pipeline at which configuration middleware is invoked.
        /// </summary>
        Configuration = -1000,

        /// <summary>
        /// The default position in the pipeline.
        /// </summary>
        Default = default,

        /// <summary>
        /// The position in the pipeline at which error reporting middleware is invoked.
        /// </summary>
        ErrorReporting = 1000,
    }

    internal enum MiddlewareOrderInternal
    {
        Startup = -4000,
        RegisterWithDotnetSuggest = -2400,
    }
}