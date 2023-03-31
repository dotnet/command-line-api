// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine.Completions;

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

            var shellTypeArgument = new CliArgument<ShellType>(nameof(ShellType));

            CompleteScriptCommand = new CliCommand("script", "Print complete script for specific shell")
            {
                shellTypeArgument
            };
            CompleteScriptCommand.SetAction(context =>
            {
                SuggestionShellScriptHandler.Handle(context.Configuration.Output, context.GetValue(shellTypeArgument));
            });

            ListCommand = new CliCommand("list")
            {
                Description = "Lists apps registered for suggestions",
            };
            ListCommand.SetAction((ctx, cancellationToken) =>
            {
                ctx.Configuration.Output.WriteLine(ShellPrefixesToMatch(_suggestionRegistration));
                return Task.CompletedTask;
            });

            GetCommand = new CliCommand("get", "Gets suggestions from the specified executable")
            {
                ExecutableOption,
                PositionOption
            };
            GetCommand.SetAction(Get);

            var commandPathOption = new CliOption<string>("--command-path") { Description = "The path to the command for which to register suggestions" };

            RegisterCommand = new CliCommand("register", "Registers an app for suggestions")
            {
                commandPathOption,
                new CliOption<string>("--suggestion-command") { Description = "The command to invoke to retrieve suggestions" }
            };

            RegisterCommand.SetAction((context, cancellationToken) =>
            {
                Register(context.GetValue(commandPathOption), context.Configuration.Output);
                return Task.CompletedTask;
            });

            var root = new CliRootCommand
            {
                ListCommand,
                GetCommand,
                RegisterCommand,
                CompleteScriptCommand,
            };
            root.TreatUnmatchedTokensAsErrors = false;
            Configuration = new CliConfiguration(root)
            {
                Directives =
                {
                    new DiagramDirective(),
                    new SuggestDirective(),
                }
            };
        }

        private CliCommand CompleteScriptCommand { get; }

        private CliCommand GetCommand { get; }

        private CliOption<FileInfo> ExecutableOption { get; } = GetExecutableOption();

        private static CliOption<FileInfo> GetExecutableOption()
        {
            var option = new CliOption<FileInfo>("--executable", "-e") { Description = "The executable to call for suggestions" };
            option.AcceptLegalFilePathsOnly();

            return option;
        }

        private CliCommand ListCommand { get; }

        private CliOption<int> PositionOption { get; } = new("--position", "-p")
        {
            Description = "The current character position on the command line",
            DefaultValueFactory = (_) => short.MaxValue
        };

        private CliCommand RegisterCommand { get; }

        public CliConfiguration Configuration { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(5000);

        public Task<int> InvokeAsync(string[] args) => Configuration.InvokeAsync(args);

        private void Register(
            string commandPath,
            TextWriter output)
        {
            var existingRegistration = _suggestionRegistration.FindRegistration(new FileInfo(commandPath));

            if (existingRegistration is null)
            {
                _suggestionRegistration.AddSuggestionRegistration(
                    new Registration(commandPath));

                output.WriteLine($"Registered {commandPath}");
            }
            else
            {
                output.WriteLine($"Registered {commandPath}");
            }
        }

        private Task<int> Get(ParseResult parseResult, CancellationToken cancellationToken)
        {
            var commandPath = parseResult.GetValue(ExecutableOption);

            Registration suggestionRegistration;
            if (commandPath.FullName == DotnetMuxer.Path.FullName)
            {
                suggestionRegistration = new Registration(commandPath.FullName);
            }
            else
            {
                suggestionRegistration = _suggestionRegistration.FindRegistration(commandPath);
            }

            var position = parseResult.GetValue(PositionOption);

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

            parseResult.Configuration.Output.Write(completions);

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