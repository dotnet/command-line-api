// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.CompletionSuggestions
{
    public class SuggestionDispatcher
    {
        private const string Position = "-p";
        private const string ExeName = "-e";

        public static string Dispatch(string[] args, ICompletionFileProvider completionFileProvider)
        {
            // CommandLine:
            // dotnet run System.CommandLine <currentCommandLine> <cursorPosition> Foo bar
            Parser parser = new ParserBuilder()
                .AddOption(Position, "the current character position on the command line",
                    position => position.ExactlyOne())
                .AddOption(ExeName, "The executible to ask for argument resolution", argument => argument
                    .LegalFilePathsOnly()
                    .ExactlyOne())
                .TreatUnmatchedTokensAsErrors(false)
                .Build();
            ParseResult parseResult = parser.Parse(args);

            // TODO Figure out when TreatUnmatchedTokensAsError(false) still puts things in the .Errors property
            /*if (parseResult.Errors.Count > 0)
            {
                throw new Exception(parseResult.ErrorText());
            }*/


            string completionTarget =
                completionFileProvider.FindCompletionRegistration(parseResult.ValueForOption<FileInfo>(ExeName));

            if (string.IsNullOrEmpty(completionTarget))
            {
                // Can't find a completion exe to call
                return string.Empty;
            }

            // Parse out path to completion target exe from config file line
            string[] keyValuePair = completionTarget.Split('=');
            if (keyValuePair.Length < 2)
            {
                throw new FormatException(
                    $"Syntax for configuration of '{completionTarget}' is not of the format '<command>=<value>'");
            }

            List<string> targetCommands = keyValuePair[1].Tokenize().ToList();

            string targetArgs = GetArgsString(parseResult, targetCommands);

            return GetCompletionSuggestions(targetCommands.First(), targetArgs);

        }

        private static string GetArgsString(ParseResult parseResult, List<string> targetCommands)
        {
            //TODO: don't just assume the callee has a "--position" argument
            return string.Join(' ',
                targetCommands[1],
                "--position",
                parseResult.ValueForOption<string>(Position),
                $"\"{string.Join(' ', parseResult.UnmatchedTokens)}\"");
        }

        public static string GetCompletionSuggestions(string exeFileName, string args, int millisecondsTimeout = 5000)
        {
            if (args == null)
            {
                args = "";
            }

            string result = "";
            // Invoke target with args
            using (var process = new Process {
                StartInfo = new ProcessStartInfo(exeFileName, args) {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            })
            {

                process.Start();

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Task<string> readToEndTask = process.StandardOutput.ReadToEndAsync();

                readToEndTask.Wait(millisecondsTimeout);

                if (readToEndTask.IsCompleted)
                {
                    result = readToEndTask.Result;
                }
                else
                {
                    readToEndTask.ContinueWith(
                        antecedentTask => antecedentTask.Wait(cancellationTokenSource.Token), TaskContinuationOptions.ExecuteSynchronously);
                    cancellationTokenSource.Cancel();
                }
            }
            return result;
        }

        private static void DisplayHelp() => throw new NotImplementedException();
    }
}
