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
                result.ErrorMessage = result.LocalizationResources.FileDoesNotExist(token.Value);
                return;
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
                result.ErrorMessage = result.LocalizationResources.DirectoryDoesNotExist(token.Value);
                return;
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
                result.ErrorMessage = result.LocalizationResources.FileOrDirectoryDoesNotExist(token.Value);
                return;
            }
        }
    }
}