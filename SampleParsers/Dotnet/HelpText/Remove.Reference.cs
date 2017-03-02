namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static partial class Remove
    {
        public static class Reference
        {
            public const string HelpText =
                @".NET Remove Project to Project reference Command

Usage: dotnet remove <PROJECT> reference [options] [args]

Arguments:
  <PROJECT>  The project file to operate on. If a file is not specified, the command will search the current directory for one.

Options:
  -h|--help                   Show help information
  -f|--framework <FRAMEWORK>  Remove reference only when targetting a specific framework

Additional Arguments:
 Project to project references to remove";
        }
    }
}