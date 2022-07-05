// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;

namespace System.CommandLine.Benchmarks.Helpers
{
    class NullConsole : IConsole
    {
        readonly NullStreamWriter _nullWriter = new();

        public IStandardStreamWriter Out => _nullWriter;
        public IStandardStreamWriter Error => _nullWriter;

        public bool IsOutputRedirected { get; } = false;
        public bool IsErrorRedirected { get; } = false;
        public bool IsInputRedirected { get; } = false;
    }
}
