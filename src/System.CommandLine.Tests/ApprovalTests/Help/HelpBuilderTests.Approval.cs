﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System.CommandLine.Help;
using System.IO;
using ApprovalTests;

namespace System.CommandLine.Tests.Help
{
    public partial class HelpBuilderTests
    {
        [Fact]
        public void Help_describes_default_values_for_complex_root_command_scenario()
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
                    Description = "the-root-arg-enum-default-description",
                    ArgumentType = typeof(FileAccess)
                },
                new Option(aliases: new string[] {"--the-root-option-no-arg", "-trna"}) {
                    Description = "the-root-option-no-arg-description",
                    Required = true
                },
                new Option<string>(
                    aliases: new string[] {"--the-root-option-no-description-default-arg", "-trondda"}, 
                    parseArgument: _ => "the-root-option--no-description-default-arg-value",
                    isDefault: true
                ),
                new Option(aliases: new string[] {"--the-root-option-no-default-arg", "-tronda"}) {
                    Description = "the-root-option-no-default-description",
                    Argument = new Argument<string>("the-root-option-arg-no-default-arg")
                    {
                        Description = "the-root-option-arg-no-default-description"
                    },
                    Required = true
                },
                new Option(aliases: new string[] {"--the-root-option-default-arg", "-troda"}) {
                    Description = "the-root-option-default-arg-description",
                    Argument = new Argument<string>("the-root-option-arg", () => "the-root-option-arg-value")
                    {
                        Description = "the-root-option-arg-description"
                    },
                },
                new Option(aliases: new string[] {"--the-root-option-enum-arg", "-troea"}) {
                    Description = "the-root-option-description",
                    Argument = new Argument<FileAccess>("the-root-option-arg", () => FileAccess.Read)
                    {
                        Description = "the-root-option-arg-description",
                    },
                },
                new Option(aliases: new string[] {"--the-root-option-required-enum-arg", "-trorea"}) {
                    Description = "the-root-option-description",
                    Argument = new Argument<FileAccess>("the-root-option-arg", () => FileAccess.Read)
                    {
                        Description = "the-root-option-arg-description",
                    },
                    Required = true
                }
            };
            command.Name = "the-root-command";

            HelpBuilder helpBuilder = GetHelpBuilder(LargeMaxWidth);
            helpBuilder.Write(command);
            var output = _console.Out.ToString();
            Approvals.Verify(output);
        }

    }
}
