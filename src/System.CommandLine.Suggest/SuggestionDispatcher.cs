// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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

            var shellTypeArgument = new Argument<ShellType>
            {
                Name = nameof(ShellType)
            };

            CompleteScriptCommand = new Command("script", "Print complete script for specific shell")
            {
                shellTypeArgument
            };
            CompleteScriptCommand.SetHandler(context =>
            {
                SuggestionShellScriptHandler.Handle(context.Console, context.ParseResult.GetValueForArgument(shellTypeArgument));
            });

            ListCommand = new Command("list")
            {
                Description = "Lists apps registered for suggestions",
            };
            ListCommand.SetHandler(ctx =>
            {
                ctx.Console.Out.WriteLine(ShellPrefixesToMatch(_suggestionRegistration));
                return Task.FromResult(0);
            });

            GetCommand = new Command("get", "Gets suggestions from the specified executable")
            {
                ExecutableOption,
                PositionOption
            };
            GetCommand.SetHandler(context => Get(context));

            var commandPathOption = new Option<string>("--command-path", "The path to the command for which to register suggestions");

            RegisterCommand = new Command("register", "Registers an app for suggestions")
            {
                commandPathOption,
                new Option<string>("--suggestion-command", "The command to invoke to retrieve suggestions")
            };

            RegisterCommand.SetHandler(context =>
            {
                Register(context.ParseResult.GetValueForOption(commandPathOption), context.Console);
                return Task.FromResult(0);
            });

            var root = new RootCommand
            {
                ListCommand,
                GetCommand,
                RegisterCommand,
                CompleteScriptCommand
            };
            root.TreatUnmatchedTokensAsErrors = false;
            Parser = new CommandLineBuilder(root)
                     .UseVersionOption()
                     .UseHelp()
                     .UseParseDirective()
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

        private Option<int> PositionOption { get; } = new(new[] { "-p", "--position" },
                                                          description: "The current character position on the command line",
                                                          getDefaultValue: () => short.MaxValue);

        private Command RegisterCommand { get; }

        public Parser Parser { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(5000);

        public Task<int> InvokeAsync(string[] args, IConsole console = null) =>
            Parser.InvokeAsync(args, console);

        private void Register(
            string commandPath,
            IConsole console)
        {
            var existingRegistration = _suggestionRegistration.FindRegistration(new FileInfo(commandPath));

            if (existingRegistration is null)
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

        private Task<int> Get(InvocationContext context)
        {
            var parseResult = context.ParseResult;
            var commandPath = parseResult.GetValueForOption(ExecutableOption);

            Registration suggestionRegistration;
            if (commandPath.FullName == DotnetMuxer.Path.FullName)
            {
                suggestionRegistration = new Registration(commandPath.FullName);
            }
            else
            {
                suggestionRegistration = _suggestionRegistration.FindRegistration(commandPath);
            }

            var position = parseResult.GetValueForOption(PositionOption);

            if (suggestionRegistration is null)
            {
                // Can't find a completion exe to call
#if DEBUG
                Program.LogDebug($"Couldn't find registration for parse result: {parseResult}");
#endif
                return Task.FromResult(0);
            }

            var targetExePath = suggestionRegistration.ExecutablePath;

            string targetArgs = FormatSuggestionArguments(
                parseResult,
                position,
                targetExePath);

#if DEBUG
            Program.LogDebug($"dotnet-suggest sending: {targetArgs}");
#endif

            string completions = _suggestionStore.GetCompletions(
                targetExePath,
                targetArgs,
                Timeout).Trim();

#if DEBUG
            Program.LogDebug($"dotnet-suggest returning: \"{completions.Replace("\r", "\\r").Replace("\n", "\\n")}\"");
#endif

            context.Console.Out.Write(completions);

            return Task.FromResult(0);
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
            var tokens = parseResult.UnmatchedTokens;

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