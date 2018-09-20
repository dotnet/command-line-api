// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.CommandLine.Invocation;

namespace System.CommandLine.Suggest
{
    public class SuggestionDispatcher
    {
        private const string Position = "-p";
        private const string ExecutingCommandOptionName = "-e";
        private const string CompletionAvailableCommands = "list";

        private readonly ISuggestionRegistration _suggestionRegistration;
        private readonly ISuggestionStore _suggestionStore;
        private readonly Parser _parser;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(5000);

        public SuggestionDispatcher(ISuggestionRegistration suggestionRegistration, ISuggestionStore suggestionStore = null)
        {
            _suggestionRegistration = suggestionRegistration ?? throw new ArgumentNullException(nameof(suggestionRegistration));
            _suggestionStore = suggestionStore ?? new SuggestionStore();

            _parser = new CommandLineBuilder()
                .UseHelp()
                .UseExceptionHandler()
                .AddCommand(CompletionAvailableCommands,
                    "list all completions available commands with space separated list",
                    cmd => cmd.OnExecute<IConsole>(c =>
                        c.Out.WriteLine(GetCompletionAvailableCommands(_suggestionRegistration))),
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
                .AddVersionOption()
                .TreatUnmatchedTokensAsErrors(false)
                .Build();
        }

        public Task<int> InvokeAsync(string[] args, IConsole console = null) =>
            _parser.InvokeAsync(args, console);

        private void RegisterCommand(string commandPath, string suggestionCommand)
        {
            _suggestionRegistration.AddSuggestionRegistration(new SuggestionRegistration(commandPath, suggestionCommand));
        }

        private void GetSuggestions(ParseResult parseResult, IConsole console)
        {
            var commandPath = parseResult.ValueForOption<FileInfo>(ExecutingCommandOptionName);

            SuggestionRegistration suggestionRegistration =
                _suggestionRegistration.FindRegistration(commandPath);

            if (suggestionRegistration == null)
            {
                // Can't find a completion exe to call
                return;
            }

            string targetArgs = FormatSuggestionArguments(parseResult, suggestionRegistration.SuggestionCommand.Tokenize().ToList());

            string suggestions = _suggestionStore.GetSuggestions(suggestionRegistration.CommandPath, targetArgs, Timeout);
            if (!string.IsNullOrWhiteSpace(suggestions))
            {
                console.Out.WriteLine(suggestions);
            }
        }

        private static string GetCompletionAvailableCommands(ISuggestionRegistration suggestionProvider)
        {
            IEnumerable<string> allFileNames = suggestionProvider.FindAllRegistrations()
                                .Select(suggestionRegistration => suggestionRegistration.CommandPath)
                                .Select(Path.GetFileNameWithoutExtension);

            return string.Join(" ", allFileNames);
        }

        private static string FormatSuggestionArguments(ParseResult parseResult, List<string> targetCommands)
        {
            var args = new List<string>() { targetCommands[1],
                "--position",
                parseResult.ValueForOption<string>(Position)};
            args.AddRange(parseResult.UnmatchedTokens);

            return string.Join(' ', args);
        }
    }
}

