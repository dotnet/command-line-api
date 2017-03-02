namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public partial class Sln
    {
        public static class Remove
        {
            public const string HelpText = @".NET Remove project(s) from a solution file Command

Usage: dotnet sln <SLN_FILE> remove [options] [args]

Arguments:
  <SLN_FILE>  Solution file to operate on. If not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Additional Arguments:
 Remove the specified project(s) from the solution. The project is not impacted.";
        }
    }
}