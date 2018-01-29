// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet;

namespace dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            File.WriteAllLines(@"console1.log",
                               args.Select(a => $"\"{a}\" ({a.Length})"));

            var result = DotNetParser.Instance.Parse(args);

            var complete = result["dotnet"]["complete"];

            var suggestions = Suggestions(complete);

            File.WriteAllLines(@"console2.log", suggestions);

            foreach (var suggestion in suggestions)
            {
                Console.WriteLine(suggestion);
            }

            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        private static string[] Suggestions(AppliedOption complete)
        {
            var input = complete.Arguments.SingleOrDefault() ?? "";

            var positionOption = complete.AppliedOptions.SingleOrDefault(a => a.Name == "position");
            if (positionOption != null)
            {
                var position = positionOption.Value<int>();

                if (position > input.Length)
                {
                    input += " ";
                }
            }

            var result = DotNetParser.Instance.Parse(input);

            return result.Suggestions()
                         .ToArray();
        }
    }
}