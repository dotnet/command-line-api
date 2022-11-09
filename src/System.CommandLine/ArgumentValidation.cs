// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="Argument" />.
    /// </summary>
    public static class ArgumentValidation
    {
        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<FileInfo> AcceptExistingOnly(this Argument<FileInfo> argument)
        {
            argument.AddValidator(Validate.FileExists);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<DirectoryInfo> AcceptExistingOnly(this Argument<DirectoryInfo> argument)
        {
            argument.AddValidator(Validate.DirectoryExists);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file or directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<FileSystemInfo> AcceptExistingOnly(this Argument<FileSystemInfo> argument)
        {
            argument.AddValidator(Validate.FileOrDirectoryExists);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to a existing files or directories.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<T> AcceptExistingOnly<T>(this Argument<T> argument)
            where T : IEnumerable<FileSystemInfo>
        {
            if (typeof(IEnumerable<FileInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.AddValidator(Validate.FileExists);
            }
            else if (typeof(IEnumerable<DirectoryInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.AddValidator(Validate.DirectoryExists);
            }
            else
            {
                argument.AddValidator(Validate.FileOrDirectoryExists);
            }

            return argument;
        }
    }
}
