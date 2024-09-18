// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;

namespace System.CommandLine.Directives;

public class ResponseSubsystem()
    : CliSubsystem("Response", SubsystemKind.Response)
{
    public bool Enabled { get; set; }

    protected internal override void Initialize(InitializationContext context)
        => context.Configuration.ResponseFileTokenReplacer = Replacer;

    public (List<string>? tokens, List<string>? errors) Replacer(string responseSourceName)
    {
        if (!Enabled)
        {
            return ([responseSourceName], null);
        }
        try
        {
            // TODO: Include checks from previous system.
            var contents = File.ReadAllText(responseSourceName);
            return (CliParser.SplitCommandLine(contents).ToList(), null);
        }
        catch
        {
            // TODO: Switch to proper errors
            return (null,
                    errors:
                    [
                        $"Failed to open response file {responseSourceName}"
                    ]);
        }
    }

    // TODO: File handling from previous system - ensure these checks are done (note: no tests caught these oversights
    /* internal static bool TryReadResponseFile(
         string filePath,
         out IReadOnlyList<string>? newTokens,
         out string? error)
     {
         try
         {
             newTokens = ExpandResponseFile(filePath).ToArray();
             error = null;
             return true;
         }
         catch (FileNotFoundException)
         {
             error = LocalizationResources.ResponseFileNotFound(filePath);
         }
         catch (IOException e)
         {
             error = LocalizationResources.ErrorReadingResponseFile(filePath, e);
         }

         newTokens = null;
         return false;

         static IEnumerable<string> ExpandResponseFile(string filePath)
         {
             var lines = File.ReadAllLines(filePath);

             for (var i = 0; i < lines.Length; i++)
             {
                 var line = lines[i];

                 foreach (var p in SplitLine(line))
                 {
                     if (GetReplaceableTokenValue(p) is { } path)
                     {
                         foreach (var q in ExpandResponseFile(path))
                         {
                             yield return q;
                         }
                     }
                     else
                     {
                         yield return p;
                     }
                 }
             }
         }

         static IEnumerable<string> SplitLine(string line)
         {
             var arg = line.Trim();

             if (arg.Length == 0 || arg[0] == '#')
             {
                 yield break;
             }

             foreach (var word in CliParser.SplitCommandLine(arg))
             {
                 yield return word;
             }
         }
     }
    */

}