namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class List
    {
        public const string HelpText =
            @".NET List Command

Usage: dotnet list [arguments] [options] [command]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  reference  Command to list project to project references

Use ""dotnet list [command] --help"" for more information about a command.";
    }
}