using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            // CommandLine:
            // dotnet run System.CommandLine <currentCommandLine> <cursorPosition>
            var parser = new ParserBuilder()
                .AddArguments(position => position.ExactlyOne())
                .AddArguments(argument => argument
                    .LegalFilePathsOnly()
                    .ExactlyOne())
                .AddArguments(remaining => remaining.ZeroOrMore())
                .Build();
            ParseResult parseResult = parser.Parse(args);

            if (parseResult.Errors.Count > 0)
            {
                throw new Exception(parseResult.ErrorText());
            }


            // Load file where items are registered
            IEnumerable<string> registrationConfigFilePaths =
                new[] { Path.Combine(Assembly.GetExecutingAssembly().Location, "System.CommandLine.Completion.json") };


            // TODO: Discuss/resolve using System.conco
            string suggestionExeFullPath = Path.GetFullPath(args[0]);

            string completionTarget = null;

            foreach (string configFilePath in registrationConfigFilePaths)
            {
                if (File.Exists(configFilePath))
                {
                    // read file
                    var configFileLines = File.ReadAllLines(configFilePath);

                    // check if args[0] exists in the file
                    completionTarget = configFileLines.SingleOrDefault(line => line.StartsWith(configFilePath));

                    if (completionTarget != null)
                    {
                        break;
                    }
                }
            }

            if (false == string.IsNullOrEmpty(completionTarget))
            {
                // Parse out path to completion target exe from config file line
                string[] keyValyePair = completionTarget.Split('=');
                if (keyValyePair.Length < 2)
                {
                    throw new Exception($"Syntax for configuration of '{ completionTarget }' is not of the format '<command>=<value>'");
                }
                var completionTargetPath = Path.GetFullPath(keyValyePair[1]);

                // Invoke target with args


            }





            // Fine the item matching args[0]

            // Invoke and redirect output
    }

        private static void DisplayHelp()
        {
            throw new NotImplementedException();
        }
    }
}
