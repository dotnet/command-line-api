using System.CommandLine.Parsing;
using System.IO;

namespace System.CommandLine;

internal static class Validate
{
    internal static void FileExists(ArgumentResult result)
    {
        for (var i = 0; i < result.Tokens.Count; i++)
        {
            var token = result.Tokens[i];

            if (!File.Exists(token.Value))
            {
                result.ReportError(result.LocalizationResources.FileDoesNotExist(token.Value));
            }
        }
    }

    internal static void DirectoryExists(ArgumentResult result)
    {
        for (var i = 0; i < result.Tokens.Count; i++)
        {
            var token = result.Tokens[i];

            if (!Directory.Exists(token.Value))
            {
                result.ReportError(result.LocalizationResources.DirectoryDoesNotExist(token.Value));
            }
        }
    }

    internal static void FileOrDirectoryExists(ArgumentResult result)
    {
        for (var i = 0; i < result.Tokens.Count; i++)
        {
            var token = result.Tokens[i];

            if (!Directory.Exists(token.Value) && !File.Exists(token.Value))
            {
                result.ReportError(result.LocalizationResources.FileOrDirectoryDoesNotExist(token.Value));
            }
        }
    }
}