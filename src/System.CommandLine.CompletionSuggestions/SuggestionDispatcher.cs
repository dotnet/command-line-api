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
    public class SuggestionDispatcher
    {
        private const string Position = "-p";
        private const string ExecutingCommandOptionName = "-e";
        private const string CompletionAvailableCommands = "list";

        private readonly ISuggestionProvider _suggestionProvider;

        private readonly Parser _parser;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(5000);

        public SuggestionDispatcher(ISuggestionProvider suggestionProvider)
        {
            _suggestionProvider = suggestionProvider ?? throw new ArgumentNullException(nameof(suggestionProvider));

            _parser = new CommandLineBuilder()
                .AddCommand(CompletionAvailableCommands,
                    "list all completions available commands with space separated list",
                    cmd => cmd.OnExecute<IConsole>(c =>
                        c.Out.WriteLine(GetCompletionAvailableCommands(_suggestionProvider))),
                    arguments: argument => argument.None())
                .AddOption(Position, "the current character position on the command line",
                    position => position.ParseArgumentsAs<string>())
                .AddOption(ExecutingCommandOptionName, "The executable to ask for argument resolution", argument => argument
                    .LegalFilePathsOnly()
                    .ParseArgumentsAs<string>())
                .OnExecute<ParseResult, IConsole>(GetSuggestions)
                .AddCommand("register", "Register a suggestion command",
                    cmd => {
                        cmd.AddOption("--command-path", "The path to the command for which to register suggestions",
                                a => a.ParseArgumentsAs<string>())
                            .AddOption("--suggestion-command", "The command to invoke to retrieve suggestions",
                                a => a.ParseArgumentsAs<string>())
                            .OnExecute<string, string>(RegisterCommand);
                    })
                .TreatUnmatchedTokensAsErrors(false)
                .Build();
        }

        public Task<int> InvokeAsync(string[] args, IConsole console = null) =>
            _parser.InvokeAsync(args, console);

        private void RegisterCommand(string commandPath, string suggestionCommand)
        {
            _suggestionProvider.AddSuggestionRegistration(new SuggestionRegistration(commandPath, suggestionCommand));
        }

        private void GetSuggestions(ParseResult parseResult, IConsole console)
        {
            var commandPath = parseResult.ValueForOption<FileInfo>(ExecutingCommandOptionName);

            SuggestionRegistration suggestionRegistration =
                _suggestionProvider.FindRegistration(commandPath);

            if (suggestionRegistration == null)
            {
                // Can't find a completion exe to call
                return;
            }

            string targetArgs = FormatSuggestionArguments(parseResult, suggestionRegistration.SuggestionCommand.Tokenize().ToList());

            string suggestions = GetSuggestions(suggestionRegistration.CommandPath, targetArgs);
            if (!string.IsNullOrWhiteSpace(suggestions))
            {
                console.Out.WriteLine(suggestions);
            }
        }

        private static string GetCompletionAvailableCommands(ISuggestionProvider suggestionProvider)
        {
            IEnumerable<string> allFileNames = suggestionProvider.FindAllRegistrations()
                                .Select(suggestionRegistration => suggestionRegistration.CommandPath)
                                .Select(Path.GetFileNameWithoutExtension);

            return string.Join(" ", allFileNames);
        }

        private static string FormatSuggestionArguments(ParseResult parseResult, List<string> targetCommands)
        {
            //TODO: don't just assume the callee has a "--position" argument
            var args = new List<string>() { targetCommands[1],
                "--position",
                parseResult.ValueForOption<string>(Position)};
            args.AddRange(parseResult.UnmatchedTokens);

            return string.Join(' ', args);
        }

        private string GetSuggestions(string exeFileName, string suggestionTargetArguments)
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

                    if (readToEndTask.Wait(Timeout))
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

