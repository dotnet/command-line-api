// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace System.CommandLine.Suggest
{
    public class SuggestionDispatcher
    {
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
                      .UseParseDirective()
                      .UseParseErrorReporting()
                      .AddCommand("list",
                                  "Lists apps registered for suggestions",
                                  cmd => cmd.OnExecute<IConsole>(c =>
                                                                     c.Out.WriteLine(GetCompletionAvailableCommands(_suggestionRegistration))),
                                  argument => argument.None())
                      .AddCommand("get",
                                  "Gets suggestions",
                                  cmd => cmd
                                         .AddOption(new[] { "-e", "--executable" },
                                                    "The executable to ask for argument resolution",
                                                    argument => argument
                                                                .LegalFilePathsOnly()
                                                                .ParseArgumentsAs<string>())
                                         .AddOption(new[] { "-p", "--position" }, "the current character position on the command line",
                                                    position => position.ParseArgumentsAs<string>())
                                         .OnExecute<ParseResult, IConsole>(GetSuggestions))
                      .AddCommand("register",
                                  "Registers an app for suggestions",
                                  cmd =>
                                  {
                                      cmd.AddOption("--command-path", "The path to the command for which to register suggestions",
                                                    a => a.ParseArgumentsAs<string>())
                                         .AddOption("--suggestion-command", "The command to invoke to retrieve suggestions",
                                                    a => a.ParseArgumentsAs<string>())
                                         .OnExecute<string, string, IConsole>(RegisterCommand);
                                  })
                      .AddVersionOption()
                      .Build();
        }

        public Task<int> InvokeAsync(string[] args, IConsole console = null) =>
            _parser.InvokeAsync(args, console);

        private void RegisterCommand(
            string commandPath, 
            string suggestionCommand, 
            IConsole console)
        {
            _suggestionRegistration.AddSuggestionRegistration(new RegistrationPair(commandPath, suggestionCommand));

            console.Out.WriteLine($"Registered {commandPath} --> {suggestionCommand}");
        }

        private void GetSuggestions(ParseResult parseResult, IConsole console)
        {
            var commandPath = parseResult.ValueForOption<FileInfo>("-e");

            var suggestionRegistration =
                _suggestionRegistration.FindRegistration(commandPath);

            if (!suggestionRegistration.HasValue)
            {
                // Can't find a completion exe to call
                return;
            }

            string targetArgs = FormatSuggestionArguments(parseResult, suggestionRegistration.Value.SuggestionCommand.Tokenize().ToList());

            string suggestions = _suggestionStore.GetSuggestions(suggestionRegistration.Value.CommandPath, targetArgs, Timeout);
            if (!string.IsNullOrWhiteSpace(suggestions))
            {
                console.Out.Write(suggestions);
            }
        }

        private static string GetCompletionAvailableCommands(ISuggestionRegistration suggestionProvider)
        {
            IEnumerable<string> allFileNames = suggestionProvider.FindAllRegistrations()
                                                                 .Select(suggestionRegistration => suggestionRegistration.CommandPath)
                                                                 .Select(Path.GetFileNameWithoutExtension);

            return string.Join(" ", allFileNames);
        }

        private static string FormatSuggestionArguments(ParseResult parseResult, IReadOnlyList<string> targetCommands)
        {
            var args = new List<string>
            {
                targetCommands[1]
            };

            args.AddRange(parseResult.UnparsedTokens);

            return string.Join(' ', args);
        }
    }
}
