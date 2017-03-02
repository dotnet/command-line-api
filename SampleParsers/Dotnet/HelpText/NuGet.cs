namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class NuGet

    {
        public const string HelpText =
            @"NuGet Command Line 4.0.0.0

Usage: dotnet nuget [options] [command]

Options:
  -h|--help                   Show help information
  --version                   Show version information
  -v|--verbosity <verbosity>  The verbosity of logging to use. Allowed values: Debug, Verbose, Information, Minimal, Warning, Error.

Commands:
  delete  Deletes a package from the server.
  locals  Clears or lists local NuGet resources such as http requests cache, packages cache or machine-wide global packages folder.
  push    Pushes a package to the server and publishes it.

Use ""dotnet nuget [command] --help"" for more information about a command.";
    }
}