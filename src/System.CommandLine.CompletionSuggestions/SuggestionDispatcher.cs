// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.CommandLine.Invocation;

namespace System.CommandLine.CompletionSuggestions
{
    public static class SuggestionDispatcher
    {
        private const int TimeoutMilliseconds = 5000;
        private const string Position = "-p";
        private const string ExeName = "-e";
        private const string CompletionAvailableCommands = "list";

        public static Parser Parser { get; } =
            new CommandLineBuilder()
                .AddCommand(CompletionAvailableCommands,
                    "list all completions available commands with space separated list",
                    cmd => cmd.OnExecute<IConsole>(c =>
                        c.Out.WriteLine(GetCompletionAvailableCommands(new SuggestionFileProvider()))),
                    arguments: argument => argument.None())
                .AddOption(Position, "the current character position on the command line",
                    position => position.ParseArgumentsAs<string>())
                .AddOption(ExeName, "The executable to ask for argument resolution", argument => argument
                    .LegalFilePathsOnly()
                    .ParseArgumentsAs<string>())
                .OnExecute<ParseResult, IConsole>(
                    (parseResult, console) =>
                        console.Out.WriteLine(Dispatch(parseResult,
                            new SuggestionFileProvider(),
                            GetSuggestions,
                            TimeoutMilliseconds)))
                .TreatUnmatchedTokensAsErrors(false)
                .Build();

        public static string Dispatch(
            ParseResult parseResult,
            ISuggestionFileProvider suggestionFileProvider,
            Func<string, string, int, string> getSuggestions,
            int timeoutMilliseconds)
        {
            var exePath = parseResult.ValueForOption<FileInfo>(ExeName);

            string suggestionRegistration =
                suggestionFileProvider.FindRegistration(exePath);

            if (string.IsNullOrWhiteSpace(suggestionRegistration))
            {
                // Can't find a completion exe to call
                return string.Empty;
            }

            string[] keyValuePair = ParseOutPathToCompletionTargetExeFromConfigFileLine(suggestionRegistration);

            List<string> targetCommands = keyValuePair[1].Tokenize().ToList();

            string targetArgs = FormatSuggestionArguments(parseResult, targetCommands);

            return getSuggestions(targetCommands.First(), targetArgs, timeoutMilliseconds);
        }

        public static string GetCompletionAvailableCommands(ISuggestionFileProvider suggestionFileProvider)
        {
            var allFileNames = suggestionFileProvider.FindAllRegistrations()
                                .Select(r => ParseOutPathToCompletionTargetExeFromConfigFileLine(r)[0])
                                .Select(Path.GetFileNameWithoutExtension);

            return string.Join(" ", allFileNames);
        }

        private static string[] ParseOutPathToCompletionTargetExeFromConfigFileLine(string suggestionRegistration)
        {
            string[] keyValuePair = suggestionRegistration.Split(new[] {'='}, 2);
            if (keyValuePair.Length < 2)
            {
                throw new FormatException(
                    $"Syntax for configuration of '{suggestionRegistration}' is not of the format '<command>=<value>'");
            }

            return keyValuePair;
        }

        private static string FormatSuggestionArguments(ParseResult parseResult, List<string> targetCommands)
        {
            //TODO: don't just assume the callee has a "--position" argument
            return string.Join(' ',
                targetCommands[1],
                "--position",
                parseResult.ValueForOption<string>(Position),
                $"{string.Join(' ', parseResult.UnmatchedTokens)}");
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
    }
}
