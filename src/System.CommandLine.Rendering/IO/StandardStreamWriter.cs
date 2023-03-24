// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace System.CommandLine.IO
{
    /// <summary>
    /// Provides methods for working with standard streams.
    /// </summary>
    public static class StandardStreamWriter
    {
        /// <summary>
        /// Creates a <see cref="TextWriter"/> that writes to the specified <see cref="IStandardStreamWriter"/>.
        /// </summary>
        public static TextWriter CreateTextWriter(this IStandardStreamWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            return new TextWriterThatWritesToStandardStreamWriter(writer);
        }

        /// <summary>
        /// Creates a <see cref="IStandardStreamWriter"/> that writes to the specified <see cref="TextWriter"/>.
        /// </summary>
        public static IStandardStreamWriter Create(TextWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            return new AnonymousStandardStreamWriter(writer.Write);
        }

        /// <summary>
        /// Appends the current environment's line terminator.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        public static void WriteLine(this IStandardStreamWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(Environment.NewLine);
        }

        /// <summary>
        /// Writes the current string value, followed by the current environment's line terminator.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(this IStandardStreamWriter writer, string value)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(value + Environment.NewLine);
        }

        private class TextWriterThatWritesToStandardStreamWriter : TextWriter
        {
            private readonly IStandardStreamWriter _writer;

            public TextWriterThatWritesToStandardStreamWriter(IStandardStreamWriter writer)
            {
                _writer = writer;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
                _writer.Write(value.ToString());
            }
            
            public override void Write(string? value)
            {
                _writer.Write(value);
            }
        }

        private class AnonymousStandardStreamWriter : IStandardStreamWriter
        {
            private readonly Action<string?> _write;

            public AnonymousStandardStreamWriter(Action<string?> write)
            {
                _write = write;
            }

            public void Write(string? value)
            {
                _write(value);
            }
        }
    }
}
