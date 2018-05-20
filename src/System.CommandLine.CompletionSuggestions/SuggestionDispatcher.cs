using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.CompletionSuggestions
{
    public static class SuggestionDispatcher
    {
        private const string Position = "-p";
        private const string ExeName = "-e";

        public static void Dispatch(string[] args)
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


            // Load file where items are registered
            IEnumerable<string> registrationConfigFilePaths =
                new[] {
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "System.CommandLine.Completion.txt")
                };


            // TODO: Discuss/resolve using System.conco

            string completionTarget = null;

            foreach (string configFilePath in registrationConfigFilePaths)
            {
                if (File.Exists(configFilePath))
                {
                    // read file
                    string[] configFileLines = File.ReadAllLines(configFilePath);

                    // check if args[0] exists in the file
                    completionTarget = configFileLines.SingleOrDefault(line =>
                        line.StartsWith(parseResult.ValueForOption<FileInfo>(ExeName).FullName));

                    if (completionTarget != null)
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(completionTarget))
            {
                // Can't find a completion exe to call
                return;
            }

            // Parse out path to completion target exe from config file line
            string[] keyValuePair = completionTarget.Split('=');
            if (keyValuePair.Length < 2)
            {
                throw new Exception(
                    $"Syntax for configuration of '{completionTarget}' is not of the format '<command>=<value>'");
            }

            List<string> targetCommands = keyValuePair[1].Tokenize().ToList();

            string targetArgs = string.Join(' ',
                targetCommands[1],
                "--position",
                parseResult.ValueForOption<string>(Position),
                $"\"{string.Join(' ', parseResult.UnmatchedTokens)}\"");

            // Invoke target with args
            var process = new Process {
                StartInfo = new ProcessStartInfo(targetCommands.First(), targetArgs) {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            process.Start();

            Console.Write(process.StandardOutput.ReadToEnd());

            process.WaitForExit(5000);

        }

        private static void DisplayHelp() => throw new NotImplementedException();
    }
}
