// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace System.CommandLine.Suggest
{
    public class SuggestionStore : ISuggestionStore
    {
        public string GetSuggestions(string exeFileName, string suggestionTargetArguments, TimeSpan timeout)
        {
            if (string.IsNullOrWhiteSpace(exeFileName))
            {
                throw new ArgumentException("message", nameof(exeFileName));
            }

            if (string.IsNullOrWhiteSpace(suggestionTargetArguments))
            {
                throw new ArgumentException("message", nameof(suggestionTargetArguments));
            }

            string result = "";

            try
            {
                // Invoke target with args
                var processStartInfo = new ProcessStartInfo(
                                           exeFileName, 
                                           suggestionTargetArguments)
                                       {
                                           UseShellExecute = false,
                                           RedirectStandardOutput = true
                                       };

                using (var process = new Process
                                     {
                                         StartInfo = processStartInfo
                                     })
                {
#if DEBUG
                    Program.LogDebug($"dotnet-suggest invoking: {exeFileName} {suggestionTargetArguments}");
#endif

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

