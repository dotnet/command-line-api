// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace System.CommandLine.Suggest
{
    public class SuggestionStore : ISuggestionStore
    {
        public string GetSuggestions(string exeFileName, string suggestionTargetArguments, TimeSpan timeout)
        {
            string result = "";

            try
            {
                // Invoke target with args
                using (var process = new Process {
                    StartInfo = new ProcessStartInfo(exeFileName, suggestionTargetArguments ?? "") {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                })
                {
                    process.Start();

                    Task<string> readToEndTask = process.StandardOutput.ReadToEndAsync();

                    if (readToEndTask.Wait(timeout))
                    {
                        result = readToEndTask.Result;
                    }
                    else
                    {
                        process.Kill();
                    }
                }
            }
            catch (Win32Exception exception)
            {
                // We don't check for the existence of exeFileName until the exception in case
                // it is a command that start process can resolve to a file name.
                if (!File.Exists(exeFileName))
                {
                    throw new ArgumentException(
                        $"Unable to find the file '{exeFileName}'", nameof(exeFileName), exception);
                }
            }
            return result;
        }
    }
}

