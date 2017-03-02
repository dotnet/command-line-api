namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class List
    {
        public static class Reference
        {
            public const string HelpText =
                @".NET Core Project-to-Project dependency viewer

Usage: dotnet list <PROJECT> reference [options]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help  Show help information";
        }
    }
}