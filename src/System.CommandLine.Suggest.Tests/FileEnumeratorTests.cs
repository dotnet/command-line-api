// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class FileEnumeratorTests
    {
        [Fact]
        public void EnumerateFilesWithoutExtension_returns_empty_when_pass_in_null()
        {
            FileEnumerator.EnumerateFilesWithoutExtension(null).Should().BeEmpty();
        }

        [Fact]
        public void EnumerateFilesWithoutExtension_returns_empty_when_directory_does_not_exist()
        {
            var path = Path.GetTempPath();
            FileEnumerator.EnumerateFilesWithoutExtension(
                new DirectoryInfo(Path.Combine(path,
                Path.GetRandomFileName(),
                "notexist")))
                .Should().BeEmpty();
        }

        [Fact]
        public void EnumerateFilesWithoutExtension_returns_files_without_extension()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(path);
                File.WriteAllText(Path.Combine(path, "dotnet-suggest"), "");
                File.WriteAllText(Path.Combine(path, "t-rex"), "");
                FileEnumerator.EnumerateFilesWithoutExtension(new DirectoryInfo(path))
                    .Should()
                    .BeEquivalentTo(
                        GlobalToolsSuggestionRegistrationTests
                        .FilesNameWithoutExtensionUnderDotnetProfileToolsExample);
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public void EnumerateFilesWithoutExtension_keeps_dotted_names_and_removes_only_exe_extension()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(path);
                File.WriteAllText(Path.Combine(path, "Example.Tools.StatusMonitor.Console"), "");
                File.WriteAllText(Path.Combine(path, "SampleUi.Automation.TestClient.exe"), "");
                FileEnumerator.EnumerateFilesWithoutExtension(new DirectoryInfo(path))
                    .Should()
                    .BeEquivalentTo("Example.Tools.StatusMonitor.Console", "SampleUi.Automation.TestClient");
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }
    }
}
