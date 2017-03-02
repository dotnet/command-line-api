namespace CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class NuGet
    {
        public static class Locals
        {
            public const string HelpText =
                @"Usage: dotnet nuget locals [arguments] [options]

Arguments:
  Cache Location(s)  Specifies the cache location(s) to list or clear.
<all | http-cache | global-packages | temp>

Options:
  -h|--help               Show help information
  --force-english-output  Forces the application to run using an invariant, English-based culture.
  -c|--clear              Clear the selected local resources or cache location(s).
  -l|--list               List the selected local resources or cache location(s).";
        }
    }
}