// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Specifies settings for response file parsing.
    /// </summary>
    public enum ResponseFileHandling
    {

        ///<summary>
        /// Each line in the file is treated as a single argument, regardless of whitespace on the line.
        ///</summary>
        ///<remarks>
        /// Empty lines and lines beginning with <c>#</c> are skipped.
        ///</remarks>
        ParseArgsAsLineSeparated,

        ///<summary>
        /// Do not parse response files. Command line tokens beginning with <c>@</c> receive no special treatment.
        ///</summary>
        Disabled
    }
}
