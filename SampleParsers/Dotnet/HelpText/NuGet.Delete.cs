namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class NuGet
    {
        public static class Delete
        {
            public const string HelpText =
                @"Usage: dotnet nuget delete [arguments] [options]

Arguments:
  [root]  The Package Id and version.

Options:
  -h|--help               Show help information
  --force-english-output  Forces the application to run using an invariant, English-based culture.
  -s|--source <source>    Specifies the server URL
  --non-interactive       Do not prompt for user input or confirmations.
  -k|--api-key <apiKey>   The API key for the server.";
        }
    }
}