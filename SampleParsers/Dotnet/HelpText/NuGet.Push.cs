namespace CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class NuGet
    {
        public static class Push
        {
            public const string HelpText =
                @"Usage: dotnet nuget push [arguments] [options]

Arguments:
  [root]  Specify the path to the package and your API key to push the package to the server.

Options:
  -h|--help                      Show help information
  --force-english-output         Forces the application to run using an invariant, English-based culture.
  -s|--source <source>           Specifies the server URL
  -ss|--symbol-source <source>   Specifies the symbol server URL. If not specified, nuget.smbsrc.net is used when pushing to nuget.org.
  -t|--timeout <timeout>         Specifies the timeout for pushing to a server in seconds. Defaults to 300 seconds (5 minutes).
  -k|--api-key <apiKey>          The API key for the server.
  -sk|--symbol-api-key <apiKey>  The API key for the symbol server.
  -d|--disable-buffering         Disable buffering when pushing to an HTTP(S) server to decrease memory usage.
  -n|--no-symbols                If a symbols package exists, it will not be pushed to a symbols server.";
        }
    }
}