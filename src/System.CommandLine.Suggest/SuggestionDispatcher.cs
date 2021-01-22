// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
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

            CompleteScriptCommand = new Command("script", "Print complete script for specific shell")
            {
                new Argument<ShellType>
                {
                    Name = nameof(ShellType)
                }
            };
            CompleteScriptCommand.Handler = CommandHandler.Create<IConsole, ShellType>(SuggestionShellScriptHandler.Handle);

            ListCommand = new Command("list")
            {
                Description = "Lists apps registered for suggestions",
                Handler = CommandHandler.Create<IConsole>(
                    c => c.Out.WriteLine(ShellPrefixesToMatch(_suggestionRegistration)))
            };

            GetCommand = new Command("get", "Gets suggestions from the specified executable")
            {
                ExecutableOption,
                PositionOption
            };
            GetCommand.Handler = CommandHandler.Create<ParseResult, IConsole>(Get);

            RegisterCommand = new Command("register", "Registers an app for suggestions")
            {
                new Option<string>("--command-path", "The path to the command for which to register suggestions"),
                new Option<string>("--suggestion-command", "The command to invoke to retrieve suggestions")
            };

            RegisterCommand.Handler = CommandHandler.Create<string, string, IConsole>(Register);

            var root = new RootCommand
            {
                ListCommand,
                GetCommand,
                RegisterCommand,
                CompleteScriptCommand
            };

            Parser = new CommandLineBuilder(root)
                     .UseVersionOption()
                     .UseHelp()
                     .UseParseDirective()
                     .UseDebugDirective()
                     .UseSuggestDirective()
                     .UseParseErrorReporting()
                     .UseExceptionHandler()
                     .Build();
        }

        private Command CompleteScriptCommand { get; }

        private Command GetCommand { get; }

        private Option<FileInfo> ExecutableOption { get; } =
            new Option<FileInfo>(new[] { "-e", "--executable" }, "The executable to call for suggestions")
                .LegalFilePathsOnly();

        private Command ListCommand { get; }

        private Option<int> PositionOption { get; } = new Option<int>(new[] { "-p", "--position" },
                                                                      description: "The current character position on the command line",
                                                                      getDefaultValue: () => short.MaxValue);

        private Command RegisterCommand { get; }

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
            var commandPath = parseResult.ValueForOption(ExecutableOption);

            Registration suggestionRegistration;
            if (commandPath.FullName == DotnetMuxer.Path.FullName)
            {
                suggestionRegistration = new Registration(commandPath.FullName);
            }
            else
            {
                suggestionRegistration = _suggestionRegistration.FindRegistration(commandPath);
            }

            var position = parseResult.ValueForOption(PositionOption);

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
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(r.ExecutablePath);

                    yield return fileNameWithoutExtension;

                    if (fileNameWithoutExtension?.StartsWith("dotnet-", StringComparison.Ordinal) == true)
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