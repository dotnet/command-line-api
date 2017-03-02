namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class Remove
    {
        public const string HelpText =
            @".NET Remove Command

Usage: dotnet remove [arguments] [options] [command]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information

Commands:
  package    Command to remove package reference.
  reference  Command to remove project to project reference

Use ""dotnet remove [command] --help"" for more information about a command.
";
    }
}