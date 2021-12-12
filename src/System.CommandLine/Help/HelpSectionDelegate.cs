// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    /// <summary>
    /// Specifies help formatting behavior for a section of command line help.
    /// </summary>
    /// <returns><see langword="true"/> if anything was written; otherwise, <see langword="false"/>.</returns>
    public delegate void HelpSectionDelegate(HelpContext context);
}