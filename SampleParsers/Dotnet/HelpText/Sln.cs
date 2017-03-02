namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class Sln
    {
        public const string HelpText =
            @".NET modify solution file command

Usage: dotnet sln [arguments] [options] [command]

Arguments:
  <SLN_FILE>  Solution file to operate on. If not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  add     Add a specified project(s) to the solution.
  list    List all projects in the solution.
  remove  Remove the specified project(s) from the solution. The project is not impacted.

Use ""dotnet sln [command] --help"" for more information about a command.
";
    }
}