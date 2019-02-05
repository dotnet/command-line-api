// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Sdk;

namespace System.CommandLine.Tests
{
    public class RemoteExecution : IDisposable
    {
        private const int FailWaitTimeoutMilliseconds = 60 * 1000;
        private string _exceptionFile;

        public RemoteExecution(Process process, string className, string methodName, string exceptionFile)
        {
            Process = process;
            ClassName = className;
            MethodName = methodName;
            _exceptionFile = exceptionFile;
        }

        public Process Process { get; private set; }
        public string ClassName { get; private set; }
        public string MethodName { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this); // before Dispose(true) in case the Dispose call throws
            Dispose(disposing: true);
        }

        private void Dispose(bool disposing)
        {
            Assert.True(disposing, $"A test {ClassName}.{MethodName} forgot to Dispose() the result of RemoteInvoke()");

            if (Process != null)
            {
                Assert.True(Process.WaitForExit(FailWaitTimeoutMilliseconds),
                    $"Timed out after {FailWaitTimeoutMilliseconds}ms waiting for remote process {Process.Id}");

                // A bit unorthodox to do throwing operations in a Dispose, but by doing it here we avoid
                // needing to do this in every derived test and keep each test much simpler.
                try
                {
                    if (File.Exists(_exceptionFile))
                    {
                        throw new RemoteExecutionException(File.ReadAllText(_exceptionFile));
                    }
                }
                finally
                {
                    if (File.Exists(_exceptionFile))
                    {
                        File.Delete(_exceptionFile);
                    }

                    // Cleanup
                    try { Process.Kill(); }
                    catch { } // ignore all cleanup errors
                }

                Process.Dispose();
                Process = null;
            }
        }

        private sealed class RemoteExecutionException : Exception
        {
            private readonly string _stackTrace;

            internal RemoteExecutionException(string stackTrace)
                : base("Remote process failed with an unhandled exception.")
            {
                _stackTrace = stackTrace;
            }

            public override string StackTrace
            {
                get => _stackTrace ?? base.StackTrace;
            }
        }
    }
}