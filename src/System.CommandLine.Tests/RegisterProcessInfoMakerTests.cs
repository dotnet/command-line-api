// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.DotNet.PlatformAbstractions;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using System.IO;

namespace System.CommandLine.Tests
{
    public class RegisterProcessInfoMakerTests
    {
        [Fact]
        public void Given_not_rooted_path_executable_full_path_it_throws()
        {
            Action a = () => RegistrationProcessInfoMaker.GetProcessStartInfoForRegistration("notright");

            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Given_executable_full_path_it_can_create_processinfo_with_correct_params()
        {
            string path;
            if (RuntimeEnvironment.OperatingSystemPlatform == Platform.Windows)
            {
                path = @"c:\my\tool\command.exe";
            }
            else
            {
                path = "/my/tool/command";
            }

            var processInfo= RegistrationProcessInfoMaker.GetProcessStartInfoForRegistration(path);

            processInfo.FileName.Should().Be("dotnet-suggest");
            processInfo.UseShellExecute.Should().BeFalse();
            processInfo.Arguments.Should().Be($"register --command-path \"{path}\" --suggestion-command \"command [suggest]\"");
        }
    }
}
