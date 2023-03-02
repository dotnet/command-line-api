﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
            var command = new Command("the-root-command", "Test description")
            {
                new Argument<string>("the-root-arg-no-description-no-default"),
                new Argument<string>("the-root-arg-no-description-default")
                {
                    DefaultValueFactory = (_) => "the-root-arg-no-description-default-value",
                },
                new Argument<string>("the-root-arg-no-default")
                {
                    Description = "the-root-arg-no-default-description",
                },
                new Argument<string>("the-root-arg")
                {
                    DefaultValueFactory = (_) => "the-root-arg-one-value",
                    Description = "the-root-arg-description"
                },
                new Argument<FileAccess>("the-root-arg-enum-default")
                {
                    DefaultValueFactory = (_) => FileAccess.Read,
                    Description = "the-root-arg-enum-default-description"
                },
                new Option<bool>("--the-root-option-no-arg", "-trna") 
                {
                    Description = "the-root-option-no-arg-description",
                    IsRequired = true
                },
                new Option<string>("--the-root-option-no-description-default-arg", "-trondda")
                {
                    DefaultValueFactory = (_) => "the-root-option--no-description-default-arg-value",
                },
                new Option<string>("--the-root-option-no-default-arg", "-tronda")
                {
                    Description = "the-root-option-no-default-description",
                    HelpName = "the-root-option-arg-no-default-arg",
                    IsRequired = true
                },
                new Option<string>("--the-root-option-default-arg", "-troda")
                {
                    DefaultValueFactory = (_) => "the-root-option-arg-value",
                    Description = "the-root-option-default-arg-description",
                    HelpName = "the-root-option-arg",
                },
                new Option<FileAccess>("--the-root-option-enum-arg", "-troea")
                {
                    DefaultValueFactory = (_) => FileAccess.Read,
                    Description = "the-root-option-description",
                },
                new Option<FileAccess>("--the-root-option-required-enum-arg", "-trorea")
                {
                    DefaultValueFactory = (_) => FileAccess.Read,
                    Description = "the-root-option-description",
                    IsRequired = true
                },
                new Option<bool>("--the-root-option-multi-line-description", "-tromld")
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
