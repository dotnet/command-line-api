// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public interface IConsole : 
        IStandardOut, 
        IStandardError, 
        IStandardIn
    {
    }

    public interface IStandardOut 
    {
        IStandardStreamWriter Out { get; }

        bool IsOutputRedirected { get; }
    }

    public interface IStandardError
    {
        IStandardStreamWriter Error { get; }

        bool IsErrorRedirected { get; }
    }

    public interface IStandardStream
    {
    }

    public interface IStandardIn : IStandardStream
    {
        bool IsInputRedirected { get; }
    }

    public interface IStandardStreamWriter : IStandardStream
    {
        void Write(string value);
    }

    public interface IStandardStreamReader : IStandardStream
    {
    }

    public interface IConsoleFactory
    {
        IConsole CreateConsole();
    }






}
