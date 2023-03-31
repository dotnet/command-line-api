// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="CliArgument" />.
    /// </summary>
    public static class ArgumentValidation
    {
        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static CliArgument<FileInfo> AcceptExistingOnly(this CliArgument<FileInfo> argument)
        {
            argument.Validators.Add(FileOrDirectoryExists<FileInfo>);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static CliArgument<DirectoryInfo> AcceptExistingOnly(this CliArgument<DirectoryInfo> argument)
        {
            argument.Validators.Add(FileOrDirectoryExists<DirectoryInfo>);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file or directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static CliArgument<FileSystemInfo> AcceptExistingOnly(this CliArgument<FileSystemInfo> argument)
        {
            argument.Validators.Add(FileOrDirectoryExists<FileSystemInfo>);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to a existing files or directories.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static CliArgument<T> AcceptExistingOnly<T>(this CliArgument<T> argument)
            where T : IEnumerable<FileSystemInfo>
        {
            if (typeof(IEnumerable<FileInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.Validators.Add(FileOrDirectoryExists<FileInfo>);
            }
            else if (typeof(IEnumerable<DirectoryInfo>).IsAssignableFrom(typeof(T)))
            {
                argument.Validators.Add(FileOrDirectoryExists<DirectoryInfo>);
            }
            else
            {
                argument.Validators.Add(FileOrDirectoryExists<FileSystemInfo>);
            }

            return argument;
        }

        private static void FileOrDirectoryExists<T>(ArgumentResult result)
            where T : FileSystemInfo
        {
            // both FileInfo and DirectoryInfo are sealed so following checks are enough
            bool checkFile = typeof(T) != typeof(DirectoryInfo);
            bool checkDirectory = typeof(T) != typeof(FileInfo);

            for (var i = 0; i < result.Tokens.Count; i++)
            {
                var token = result.Tokens[i];

                if (checkFile && checkDirectory)
                {
#if NET7_0_OR_GREATER
                    if (!Path.Exists(token.Value))
#else
                    if (!Directory.Exists(token.Value) && !File.Exists(token.Value))
#endif
                    {
                        result.AddError(LocalizationResources.FileOrDirectoryDoesNotExist(token.Value));
                    }
                }
                else if (checkDirectory && !Directory.Exists(token.Value))
                {
                    result.AddError(LocalizationResources.DirectoryDoesNotExist(token.Value));
                }
                else if (checkFile && !Directory.Exists(token.Value) && !File.Exists(token.Value))
                {
                    result.AddError(LocalizationResources.FileDoesNotExist(token.Value));
                }
            }
        }
    }
}
