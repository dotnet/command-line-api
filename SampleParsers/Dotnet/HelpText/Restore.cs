namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.HelpText
{
    public static class Restore
    {
        public const string HelpText =
            @".NET dependency restorer

Usage: restore [arguments] [options] [args]

Arguments:
  [PROJECT]  Optional path to a project file or MSBuild arguments.

Options:
  -h|--help                          Show help information
  -s|--source <SOURCE>               Specifies a NuGet package source to use during the restore.
  -r|--runtime <RUNTIME_IDENTIFIER>  Target runtime to restore packages for.
  --packages <PACKAGES_DIRECTORY>    Directory to install packages in.
  --disable-parallel                 Disables restoring multiple projects in parallel.
  --configfile <FILE>                The NuGet configuration file to use.
  --no-cache                         Do not cache packages and http requests.
  --ignore-failed-sources            Treat package source failures as warnings.
  --no-dependencies                  Set this flag to ignore project to project references and only restore the root project
  -v|--verbosity                     Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]

Additional Arguments:
 Any extra options that should be passed to MSBuild. See 'dotnet msbuild -h' for available options.";
    }
}