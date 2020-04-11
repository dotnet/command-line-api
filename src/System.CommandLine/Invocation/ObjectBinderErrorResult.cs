// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Class used to report failures to find a specific alias among the Options and
    /// Arguments of a Command
    /// </summary>
    public class ObjectBinderErrorResult : IInvocationResult
    {
        private readonly string _failedAlias;
        private readonly bool _forOptions;
        private readonly string _generalError;

        /// <summary>
        /// Create an instance tied to a particular alias
        /// </summary>
        /// <param name="failedAlias">the alias which could not be found</param>
        /// <param name="forOptions">true to indicate Options were being searched, false to indicate Arguments</param>
        public ObjectBinderErrorResult( string failedAlias, bool forOptions = false )
        {
            _failedAlias = failedAlias;
            _forOptions = forOptions;
        }

        /// <summary>
        /// Create a generalized error instance, unrelated to any particular alias
        /// </summary>
        /// <param name="error">the error message</param>
        public ObjectBinderErrorResult( string error )
        {
            _generalError = error;
        }

        public void Apply(InvocationContext context)
        {
            context.Console.ResetTerminalForegroundColor();
            context.Console.SetTerminalForegroundRed();

            context.Console.Error.WriteLine(
                string.IsNullOrEmpty( _generalError )
                    ? $"Could not find matching {( _forOptions ? "Option" : "Argument" )} for alias '{_failedAlias}'"
                    : _generalError
            );

            context.Console.Error.WriteLine();

            context.ResultCode = 1;

            context.Console.ResetTerminalForegroundColor();
        }
    }
}
