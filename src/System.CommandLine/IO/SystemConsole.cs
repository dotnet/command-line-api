// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.IO
{
    /// <summary>
    /// Provides access to the standard streams via <see cref="System.Console"/>.
    /// </summary>
    public class SystemConsole : IConsole
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SystemConsole"/>.
        /// </summary>
        public SystemConsole()
        {
            Error = StandardErrorStreamWriter.Instance;
            Out = StandardOutStreamWriter.Instance;
        }

        /// <inheritdoc />
        public IStandardStreamWriter Error { get; }

        /// <inheritdoc />
        public bool IsErrorRedirected => Console.IsErrorRedirected;

        /// <inheritdoc />
        public IStandardStreamWriter Out { get; }

        /// <inheritdoc />
        public bool IsOutputRedirected => Console.IsOutputRedirected;

        /// <inheritdoc />
        public bool IsInputRedirected => Console.IsInputRedirected;

        internal int GetWindowWidth() => IsOutputRedirected ? int.MaxValue : Console.WindowWidth;

        private struct StandardErrorStreamWriter : IStandardStreamWriter
        {
            public static readonly StandardErrorStreamWriter Instance = new();

            public void Write(string? value) => Console.Error.Write(value);
        }

        private struct StandardOutStreamWriter : IStandardStreamWriter
        {
            public static readonly StandardOutStreamWriter Instance = new();

            public void Write(string? value) => Console.Out.Write(value);
        }
    }
}