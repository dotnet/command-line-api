// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Suggest
{
    public class SuggestionShellScriptException : Exception
    {
        public SuggestionShellScriptException()
        {
        }

        public SuggestionShellScriptException(string message) : base(message)
        {
        }

        public SuggestionShellScriptException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
