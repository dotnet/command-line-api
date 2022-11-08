// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Completions;
using System.IO;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="Option" />.
    /// </summary>
    public static class OptionValidationExtensions
    {
        /// <summary>
        /// Configures an option to accept only values corresponding to an existing file.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<FileInfo> AcceptExistingOnly(this Option<FileInfo> option)
        {
            option.Argument.AddValidator(Validate.FileExists);
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<DirectoryInfo> AcceptExistingOnly(this Option<DirectoryInfo> option)
        {
            option.Argument.AddValidator(Validate.DirectoryExists);
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to an existing file or directory.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<FileSystemInfo> AcceptExistingOnly(this Option<FileSystemInfo> option)
        {
            option.Argument.AddValidator(Validate.FileOrDirectoryExists);
            return option;
        }

        /// <summary>
        /// Configures an option to accept only values corresponding to a existing files or directories.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <returns>The option being extended.</returns>
        public static Option<T> AcceptExistingOnly<T>(this Option<T> option)
            where T : IEnumerable<FileSystemInfo>
        {
            if (option.Argument is Argument<T> arg)
            {
                arg.AcceptExistingOnly();
            }

            return option;
        }
    }
}