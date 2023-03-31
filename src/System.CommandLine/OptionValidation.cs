// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="CliOption" />.
    /// </summary>
    public static class OptionValidation
    {
        /// <summary>
        /// Configures an option to accept only values corresponding to an existing file.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static CliOption<FileInfo> AcceptExistingOnly(this CliOption<FileInfo> option)
        {
            option._argument.AcceptExistingOnly();

            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static CliOption<DirectoryInfo> AcceptExistingOnly(this CliOption<DirectoryInfo> option)
        {
            option._argument.AcceptExistingOnly();
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to an existing file or directory.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static CliOption<FileSystemInfo> AcceptExistingOnly(this CliOption<FileSystemInfo> option)
        {
            option._argument.AcceptExistingOnly();
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to a existing files or directories.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static CliOption<T> AcceptExistingOnly<T>(this CliOption<T> option)
            where T : IEnumerable<FileSystemInfo>
        {
            option._argument.AcceptExistingOnly();

            return option;
        }
    }
}