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

        public SuggestionDispatcher(ISuggestionRegistration suggestionRegistration, ISuggestionStore suggestionStore = null)
        {
            _suggestionRegistration = suggestionRegistration ?? throw new ArgumentNullException(nameof(suggestionRegistration));
            _suggestionStore = suggestionStore ?? new SuggestionStore();

            Parser = new CommandLineBuilder()
                      .UseVersionOption()
                      .UseHelp()
                      .UseParseDirective()
                      .UseDebugDirective()
                      .UseSuggestDirective()
                      .UseParseErrorReporting()
                      .UseExceptionHandler()

                      .AddCommand(ListCommand())
                      .AddCommand(GetCommand())
                      .AddCommand(RegisterCommand())
                      .AddCommand(CompleteScriptCommand())
                   
                      .Build();

            Command GetCommand() =>
                new Command("get",
                            "Gets suggestions from the specified executable",
                            new[] { ExecutableOption(), PositionOption() })
                {
                    Handler = CommandHandler.Create<ParseResult, IConsole>(Get)
                };

            Option ExecutableOption() =>
                new Option(new[] { "-e", "--executable" },
                           "The executable to call for suggestions",
                           new Argument<string>().LegalFilePathsOnly());

            Option PositionOption() =>
                new Option(new[] { "-p", "--position" },
                           "The current character position on the command line",
                           new Argument<int>());

            Command ListCommand() =>
                new Command(
                    "list",
                    "Lists apps registered for suggestions")
                {
                    Handler = CommandHandler.Create<IConsole>(
                        c => c.Out.WriteLine(ShellPrefixesToMatch(_suggestionRegistration)))
                };

            Command CompleteScriptCommand() =>
                new Command(
                    "complete-script",
                    "Print complete script for specific shell",
                    new[]
                    {
                        new Option("--shell",
                            "Shell name",
                            new Argument<string>())
                    })
                {
                    Handler = CommandHandler.Create<IConsole, string>(
                        SuggestionShellScriptHandler.Handle)
                };

            Command RegisterCommand() =>
                new Command("register",
                            "Registers an app for suggestions",
                            new[] { CommandPathOption(), SuggestionCommandOption() })
                {
                    Handler = CommandHandler.Create<string, string, IConsole>(Register)
                };

            Option CommandPathOption() =>
                new Option("--command-path",
                           "The path to the command for which to register suggestions",
                           new Argument<string>());

            Option SuggestionCommandOption() =>
                new Option("--suggestion-command",
                           "The command to invoke to retrieve suggestions",
                           new Argument<string>());
        }

        public Parser Parser { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(5000);

        public Task<int> InvokeAsync(string[] args, IConsole console = null) =>
            Parser.InvokeAsync(args, console);

        private void Register(
            string commandPath,
            string suggestionCommand,
            IConsole console)
        {
            var existingRegistration = _suggestionRegistration.FindRegistration(new FileInfo(commandPath));

            if (existingRegistration == null)
            {
                _suggestionRegistration.AddSuggestionRegistration(
                    new Registration(commandPath));

                console.Out.WriteLine($"Registered {commandPath}");
            }
            else
            {
                console.Out.WriteLine($"Registered {commandPath}");
            }
        }

        private void Get(ParseResult parseResult, IConsole console)
        {
            var commandPath = parseResult.ValueForOption<FileInfo>("-e");

            Registration suggestionRegistration;
            if (commandPath.FullName == DotnetMuxer.Path.FullName)
            {
                suggestionRegistration = new Registration(commandPath.FullName);
            }
            else
            {
                suggestionRegistration = _suggestionRegistration.FindRegistration(commandPath);
            }

            var position = parseResult.CommandResult["position"]?.GetValueOrDefault<int>() ?? short.MaxValue;

            if (suggestionRegistration == null)
            {
                // Can't find a completion exe to call
#if DEBUG
                Program.LogDebug($"Couldn't find registration for parse result: {parseResult}");
#endif
                return;
            }

            var targetExePath = suggestionRegistration.ExecutablePath;

            string targetArgs = FormatSuggestionArguments(
                parseResult,
                position,
                targetExePath);

#if DEBUG
            Program.LogDebug($"dotnet-suggest sending: {targetArgs}");
#endif

            string suggestions = _suggestionStore.GetSuggestions(
                targetExePath,
                targetArgs,
                Timeout).Trim();

#if DEBUG
            Program.LogDebug($"dotnet-suggest returning: \"{suggestions.Replace("\r", "\\r").Replace("\n", "\\n")}\"");
#endif

            console.Out.Write(suggestions);
        }

        private static string ShellPrefixesToMatch(
            ISuggestionRegistration suggestionProvider)
        {
            var registrations = suggestionProvider.FindAllRegistrations();

            return string.Join(Environment.NewLine, Prefixes());

            IEnumerable<string> Prefixes()
            {

                foreach (var r in registrations)
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension (r.ExecutablePath);

                    yield return fileNameWithoutExtension;

                    if (fileNameWithoutExtension.StartsWith("dotnet-", StringComparison.Ordinal))
                    {
                        yield return "dotnet " + fileNameWithoutExtension.Substring("dotnet-".Length);
                    }
                }
            }
        }

        public static string FormatSuggestionArguments(
            ParseResult parseResult,
            int position,
            string targetExeName)
        {
            var tokens = parseResult.UnparsedTokens;

            var commandLine = tokens.FirstOrDefault() ?? "";

            targetExeName = Path.GetFileName(targetExeName).RemoveExeExtension();

            int offset = 0;

            if (targetExeName == "dotnet")
            {
                // e.g. 
                int? endOfWhitespace = null;
                int? endOfSecondtoken = null;

                var choppedCommandLine = commandLine;

                for (var i = "dotnet".Length; i < commandLine.Length; i++)
                {
                    if (!char.IsWhiteSpace(commandLine[i]))
                    {
                        endOfWhitespace = i;
                        break;
                    }
                }

                if (endOfWhitespace != null)
                {
                    for (var i = endOfWhitespace.Value; i < commandLine.Length; i++)
                    {
                        if (char.IsWhiteSpace(commandLine[i]))
                        {
                            endOfSecondtoken = i;
                            break;
                        }
                    }

                    if (endOfSecondtoken != null)
                    {
                        for (var i = endOfSecondtoken.Value; i < commandLine.Length; i++)
                        {
                            if (!char.IsWhiteSpace(commandLine[i]))
                            {
                                choppedCommandLine = commandLine.Substring(i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        choppedCommandLine = "";
                    }
                }
                else
                {
                    choppedCommandLine = "";
                }

                if (choppedCommandLine.Length > 0)
                {
                    offset = commandLine.Length - choppedCommandLine.Length;
                }
                else
                {
                    offset = position;
                }

                commandLine = choppedCommandLine;
            }
            else if (commandLine.StartsWith(targetExeName))
            {
                if (commandLine.Length > targetExeName.Length)
                {
                    commandLine = commandLine.Substring(targetExeName.Length + 1);
                }
                else
                {
                    commandLine = "";
                }

                offset = targetExeName.Length + 1;
            }

            position = position - offset;

            var suggestDirective = $"[suggest:{position}]";

            return $"{suggestDirective} \"{commandLine.Escape()}\"";
        }
    }
}
