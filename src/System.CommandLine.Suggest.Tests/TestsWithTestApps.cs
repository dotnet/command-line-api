using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Suggest.Tests;

[Collection("TestsWithTestApps")]
public class TestsWithTestApps : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly FileInfo EndToEndTestApp;
    protected readonly FileInfo WaitAndFailTestApp;
    protected readonly FileInfo DotnetSuggest;
    protected readonly (string, string)[] EnvironmentVariables;
    private readonly DirectoryInfo _dotnetHostDir = DotnetMuxer.Path.Directory;
    private static string _testRoot;

    protected TestsWithTestApps(ITestOutputHelper output)
    {
        Output = output;

        // delete sentinel files for TestApps in order to trigger registration when it's run
        var sentinelsDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "system-commandline-sentinel-files"));

        if (sentinelsDir.Exists)
        {
            var sentinels = sentinelsDir
                .EnumerateFiles()
                .Where(f => f.Name.Contains("EndToEndTestApp") || f.Name.Contains("WaitAndFailTestApp"));

            foreach (var sentinel in sentinels)
            {
                sentinel.Delete();
            }
        }

        var currentDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestAssets");

        EndToEndTestApp = new DirectoryInfo(currentDirectory)
            .GetFiles("EndToEndTestApp".ExecutableName())
            .SingleOrDefault();

        WaitAndFailTestApp = new DirectoryInfo(currentDirectory)
            .GetFiles("WaitAndFailTestApp".ExecutableName())
            .SingleOrDefault();
            
        DotnetSuggest = new DirectoryInfo(currentDirectory)
            .GetFiles("dotnet-suggest".ExecutableName())
            .SingleOrDefault();

        PrepareTestHomeDirectoryToAvoidPolluteBuildMachineHome();

        EnvironmentVariables = new[] {
            ("DOTNET_ROOT", _dotnetHostDir.FullName),
            ("INTERNAL_TEST_DOTNET_SUGGEST_HOME", _testRoot)};
    }

    public void Dispose()
    {
        if (_testRoot != null && Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, recursive: true);
        }
    }

    private static void PrepareTestHomeDirectoryToAvoidPolluteBuildMachineHome()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_testRoot);
    }
}