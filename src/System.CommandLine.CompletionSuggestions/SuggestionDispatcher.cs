// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace System.CommandLine.CompletionSuggestions
{
    public class SuggestionDispatcher
    {
        private const int TimeoutMilliseconds = 5000;
        private const string Position = "-p";
        private const string ExeName = "-e";

        public static string Dispatch(
            string[] args,
            ISuggestionFileProvider suggestionFileProvider,
            int timeoutMilliseconds = TimeoutMilliseconds)
        {
            // CommandLine:
            // dotnet run System.CommandLine <currentCommandLine> <cursorPosition> Foo bar
            Parser parser = new CommandLineBuilder()
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

            string suggestionRegistration =
                suggestionFileProvider.FindRegistration(parseResult.ValueForOption<FileInfo>(ExeName));

            if (string.IsNullOrWhiteSpace(suggestionRegistration))
            {
                // Can't find a completion exe to call
                return string.Empty;
            }

            // Parse out path to completion target exe from config file line
            string[] keyValuePair = suggestionRegistration.Split(new[] { '=' }, 2);
            if (keyValuePair.Length < 2)
            {
                throw new FormatException(
                    $"Syntax for configuration of '{suggestionRegistration}' is not of the format '<command>=<value>'");
            }

            List<string> targetCommands = keyValuePair[1].Tokenize().ToList();

            string targetArgs = FormatSuggestionArguments(parseResult, targetCommands);

            return GetSuggestions(targetCommands.First(), targetArgs, timeoutMilliseconds);

        }

        private static string FormatSuggestionArguments(ParseResult parseResult, List<string> targetCommands)
        {
            //TODO: don't just assume the callee has a "--position" argument
            return string.Join(' ',
                targetCommands[1],
                "--position",
                parseResult.ValueForOption<string>(Position),
                $"\"{string.Join(' ', parseResult.UnmatchedTokens)}\"");
        }

        public static string GetSuggestions(string exeFileName,
                                            string suggestionTargetArguments,
                                            int millisecondsTimeout = TimeoutMilliseconds)
        {
            if (suggestionTargetArguments == null)
            {
                suggestionTargetArguments = "";
            }

            string result = "";

            try
            {
                // Invoke target with args
                using (var process = new Process {
                    StartInfo = new ProcessStartInfo(exeFileName, suggestionTargetArguments) {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                })
                {
                    process.Start();

                    Task<string> readToEndTask = process.StandardOutput.ReadToEndAsync();

                    readToEndTask.Wait(millisecondsTimeout);

                    if (readToEndTask.IsCompleted)
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

        private static void DisplayHelp() => throw new NotImplementedException();
    }
}
