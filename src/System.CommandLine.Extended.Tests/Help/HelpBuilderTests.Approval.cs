// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;

namespace System.CommandLine.Tests.Help
{
    public partial class HelpBuilderTests
    {
        [Fact]
        [UseReporter(typeof(DiffReporter))]
        public void Help_layout_has_not_changed()
        {
            var command = new CliCommand("the-root-command", "Test description")
            {
                new CliArgument<string>("the-root-arg-no-description-no-default"),
                new CliArgument<string>("the-root-arg-no-description-default")
                {
                    DefaultValueFactory = (_) => "the-root-arg-no-description-default-value",
                },
                new CliArgument<string>("the-root-arg-no-default")
                {
                    Description = "the-root-arg-no-default-description",
                },
                new CliArgument<string>("the-root-arg")
                {
                    DefaultValueFactory = (_) => "the-root-arg-one-value",
                    Description = "the-root-arg-description"
                },
                new CliArgument<FileAccess>("the-root-arg-enum-default")
                {
                    DefaultValueFactory = (_) => FileAccess.Read,
                    Description = "the-root-arg-enum-default-description"
                },
                new CliOption<bool>("--the-root-option-no-arg", "-trna") 
                {
                    Description = "the-root-option-no-arg-description",
                    Required = true
                },
                new CliOption<string>("--the-root-option-no-description-default-arg", "-trondda")
                {
                    DefaultValueFactory = (_) => "the-root-option--no-description-default-arg-value",
                },
                new CliOption<string>("--the-root-option-no-default-arg", "-tronda")
                {
                    Description = "the-root-option-no-default-description",
                    HelpName = "the-root-option-arg-no-default-arg",
                    Required = true
                },
                new CliOption<string>("--the-root-option-default-arg", "-troda")
                {
                    DefaultValueFactory = (_) => "the-root-option-arg-value",
                    Description = "the-root-option-default-arg-description",
                    HelpName = "the-root-option-arg",
                },
                new CliOption<FileAccess>("--the-root-option-enum-arg", "-troea")
                {
                    DefaultValueFactory = (_) => FileAccess.Read,
                    Description = "the-root-option-description",
                },
                new CliOption<FileAccess>("--the-root-option-required-enum-arg", "-trorea")
                {
                    DefaultValueFactory = (_) => FileAccess.Read,
                    Description = "the-root-option-description",
                    Required = true
                },
                new CliOption<bool>("--the-root-option-multi-line-description", "-tromld")
                {
                    Description = "the-root-option\r\nmulti-line\ndescription"
                },
            };

            StringWriter writer = new();
            GetHelpBuilder(LargeMaxWidth).Write(command, writer);
            Approvals.Verify(writer.ToString());
        }
    }
}
