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
            var command = new RootCommand(description: "Test description")
            {
                new Argument<string>("the-root-arg-no-description-no-default"),
                new Argument<string>("the-root-arg-no-description-default",
                    argResult => "the-root-arg-no-description-default-value",
                    isDefault: true),
                new Argument<string>("the-root-arg-no-default")
                {
                    Description = "the-root-arg-no-default-description",
                },
                new Argument<string>("the-root-arg", () => "the-root-arg-one-value")
                {
                    Description = "the-root-arg-description"
                },
                new Argument<FileAccess>("the-root-arg-enum-default", () => FileAccess.Read)
                {
                    Description = "the-root-arg-enum-default-description"
                },
                new Option<bool>(aliases: new string[] {"--the-root-option-no-arg", "-trna"}) {
                    Description = "the-root-option-no-arg-description",
                    IsRequired = true
                },
                new Option<string>(
                    aliases: new string[] {"--the-root-option-no-description-default-arg", "-trondda"}, 
                    parseArgument: _ => "the-root-option--no-description-default-arg-value",
                    isDefault: true
                ),
                new Option<string>(aliases: new string[] {"--the-root-option-no-default-arg", "-tronda"}) {
                    Description = "the-root-option-no-default-description",
                    ArgumentHelpName = "the-root-option-arg-no-default-arg",
                    IsRequired = true
                },
                new Option<string>(aliases: new string[] {"--the-root-option-default-arg", "-troda"}, () => "the-root-option-arg-value") 
                {
                    Description = "the-root-option-default-arg-description",
                    ArgumentHelpName = "the-root-option-arg",
                },
                new Option<FileAccess>(aliases: new string[] {"--the-root-option-enum-arg", "-troea"}, () => FileAccess.Read) 
                {
                    Description = "the-root-option-description",
                },
                new Option<FileAccess>(aliases: new string[] {"--the-root-option-required-enum-arg", "-trorea"}, () => FileAccess.Read) 
                {
                    Description = "the-root-option-description",
                    IsRequired = true
                },
                new Option<bool>(aliases: new string[] {"--the-root-option-multi-line-description", "-tromld"}) {
                    Description = "the-root-option\r\nmulti-line\ndescription"
                }
            };
            command.Name = "the-root-command";

            StringWriter writer = new();
            GetHelpBuilder(LargeMaxWidth).Write(command, writer);
            Approvals.Verify(writer.ToString());
        }
    }
}
