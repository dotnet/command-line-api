namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Publish
    {
        public const string HelpText = @".NET Publisher

Usage: dotnet publish [arguments] [options] [args]

Arguments:
  <PROJECT>  The MSBuild project file to publish. If a project file is not specified, MSBuild searches the current working directory for a file that has a file extension that ends in `proj` and uses that file.

Options:
  -h|--help                           Show help information
  -f|--framework <FRAMEWORK>          Target framework to publish for. The target framework has to be specified in the project file.
  -r|--runtime <RUNTIME_IDENTIFIER>   Publish the project for a given runtime. This is used when creating self-contained deployment. Default is to publish a framework-dependent app.
  -o|--output <OUTPUT_DIR>            Output directory in which to place the published artifacts.
  -c|--configuration <CONFIGURATION>  Configuration to use for building the project.  Default for most projects is  ""Debug"".
  --version-suffix <VERSION_SUFFIX>   Defines the value for the $(VersionSuffix) property in the project.
  -v|--verbosity                      Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.";
    }
}