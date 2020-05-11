// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.IO
{
    public static class StandardStreamWriter
    {
        public static IStandardStreamWriter Create(TextWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            return new AnonymousStandardStreamWriter(writer.Write);
        }

        public static void WriteLine(this IStandardStreamWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(Environment.NewLine);
        }

        public static void WriteLine(this IStandardStreamWriter writer, string value)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(value);
            writer.Write(Environment.NewLine);
        }

        private class AnonymousStandardStreamWriter : IStandardStreamWriter
        {
            private readonly Action<string> _write;

            public AnonymousStandardStreamWriter(Action<string> write)
            {
                _write = write;
            }

            public void Write(string value)
            {
                _write(value);
            }
        }
    }
}
