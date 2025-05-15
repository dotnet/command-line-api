// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
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
            argument.Validators.Add(FileOrDirectoryExists<FileInfo>);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<DirectoryInfo> AcceptExistingOnly(this Argument<DirectoryInfo> argument)
        {
            argument.Validators.Add(FileOrDirectoryExists<DirectoryInfo>);
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file or directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<FileSystemInfo> AcceptExistingOnly(this Argument<FileSystemInfo> argument)
        {
            argument.Validators.Add(FileOrDirectoryExists<FileSystemInfo>);
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

        /// <summary>
        /// Configures the argument to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        public static Argument<T> AcceptLegalFileNamesOnly<T>(this Argument<T> argument)
        {
            argument.Validators.Add(static result =>
            {
                var invalidFileNameChars = Path.GetInvalidFileNameChars();

                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidFileNameChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        result.AddError(LocalizationResources.InvalidCharactersInFileName(token.Value[invalidCharactersIndex]));
                    }
                }
            });

            return argument;
        }


        /// <summary>
        /// Configures the argument to accept only values representing legal file paths.
        /// </summary>
        public static Argument<T> AcceptLegalFilePathsOnly<T>(this Argument<T> argument)
        {
            argument.Validators.Add(static result =>
            {
                var invalidPathChars = Path.GetInvalidPathChars();

                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];

                    // File class no longer check invalid character
                    // https://blogs.msdn.microsoft.com/jeremykuhne/2018/03/09/custom-directory-enumeration-in-net-core-2-1/
                    var invalidCharactersIndex = token.Value.IndexOfAny(invalidPathChars);

                    if (invalidCharactersIndex >= 0)
                    {
                        result.AddError(LocalizationResources.InvalidCharactersInPath(token.Value[invalidCharactersIndex]));
                    }
                }
            });

            return argument;
        }

        /// <summary>
        /// Configures the argument to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <param name="values">The values that are allowed for the argument.</param>
        public static Argument<T> AcceptOnlyFromAmong<T>(
            this Argument<T> argument, 
            params string[] values)
        {
            if (values is not null && values.Length > 0)
            {
                argument.Validators.Clear();
                argument.Validators.Add(UnrecognizedArgumentError);
                argument.CompletionSources.Clear();
                argument.CompletionSources.Add(values);
            }

            return argument;
            
            void UnrecognizedArgumentError(ArgumentResult argumentResult)
            {
                for (var i = 0; i < argumentResult.Tokens.Count; i++)
                {
                    var token = argumentResult.Tokens[i];

                    if (token.Symbol is null || token.Symbol == argument)
                    {
                        if (Array.IndexOf(values, token.Value) < 0)
                        {
                            argumentResult.AddError(LocalizationResources.UnrecognizedArgument(token.Value, values));
                        }
                    }
                }
            }
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
