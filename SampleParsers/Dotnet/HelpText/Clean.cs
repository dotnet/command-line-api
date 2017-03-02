namespace CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Clean
    {
        public const string HelpText =
            @".NET Clean Command

Usage: dotnet clean [arguments] [options] [args]

Arguments:
  <PROJECT>  The MSBuild project file to build. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in `proj` and uses that file.

Options:
  -h|--help                           Show help information
  -o|--output <OUTPUT_DIR>            Directory in which the build outputs have been placed.
  -f|--framework <FRAMEWORK>          Clean a specific framework.
  -c|--configuration <CONFIGURATION>  Clean a specific configuration.
  -v|--verbosity                      Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.
";
    }
}