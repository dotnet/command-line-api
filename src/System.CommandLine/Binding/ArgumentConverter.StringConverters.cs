// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.Binding;

internal static partial class ArgumentConverter
{
    private static readonly Dictionary<Type, TryConvertString> _stringConverters = new()
    {
        [typeof(bool)] = (string token, out object? value) =>
        {
            if (bool.TryParse(token, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(bool?)] = (string token, out object? value) =>
        {
            if (bool.TryParse(token, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(DateTime)] = (string input, out object? value) =>
        {
            if (DateTime.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(DateTime?)] = (string input, out object? value) =>
        {
            if (DateTime.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(DateTimeOffset)] = (string input, out object? value) =>
        {
            if (DateTimeOffset.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(DateTimeOffset?)] = (string input, out object? value) =>
        {
            if (DateTimeOffset.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },


        [typeof(decimal)] = (string input, out object? value) =>
        {
            if (decimal.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(decimal?)] = (string input, out object? value) =>
        {
            if (decimal.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(DirectoryInfo)] = (string path, out object? value) =>
        {
            value = new DirectoryInfo(path);
            return true;
        },

        [typeof(double)] = (string input, out object? value) =>
        {
            if (double.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(double?)] = (string input, out object? value) =>
        {
            if (double.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(FileInfo)] = (string path, out object? value) =>
        {
            value = new FileInfo(path);
            return true;
        },

        [typeof(FileSystemInfo)] = (string path, out object? value) =>
        {
            if (Directory.Exists(path))
            {
                value = new DirectoryInfo(path);
            }
            else if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                     path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                value = new DirectoryInfo(path);
            }
            else
            {
                value = new FileInfo(path);
            }

            return true;
        },

        [typeof(float)] = (string input, out object? value) =>
        {
            if (float.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },
        
        [typeof(float?)] = (string input, out object? value) =>
        {
            if (float.TryParse(input, out var parsed))
            {
                value = parsed;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(int)] = (string token, out object? value) =>
        {
            if (int.TryParse(token, out var intValue))
            {
                value = intValue;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(int?)] = (string token, out object? value) =>
        {
            if (int.TryParse(token, out var intValue))
            {
                value = intValue;
                return true;
            }

            value = default;
            return false;
        },
        
        [typeof(long)] = (string token, out object? value) =>
        {
            if (long.TryParse(token, out var longValue))
            {
                value = longValue;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(long?)] = (string token, out object? value) =>
        {
            if (long.TryParse(token, out var longValue))
            {
                value = longValue;
                return true;
            }

            value = default;
            return false;
        },

        [typeof(string)] = (string input, out object? value) =>
        {
            value = input;
            return true;
        },

        [typeof(Uri)] = (string input, out object? value) =>
        {
            if (Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out var uri))
            {
                value = uri;
                return true;
            }

            value = default;
            return false;
        },
    };
}