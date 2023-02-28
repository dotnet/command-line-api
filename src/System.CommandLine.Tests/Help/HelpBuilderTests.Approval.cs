// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using System.Collections.Generic;
using System.Linq;

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
                new Argument<string>("arg-just-name"),
                new Argument<string>("arg-name-and-description", "description1"),
                new Argument<string>("arg-name-and-default-value-string", argResult => "default2", isDefault: true),
                new Argument<FileAccess>("arg-name-and-default-value-enum", () => FileAccess.Read),
                new Argument<string>("arg-name-and-description-and-default-value", () => "default4", "description4"),
            };

            foreach (Option notRequired in CreateOptions(firstAlias: 'a'))
            {
                command.Options.Add(notRequired);
            }

            foreach (Option required in CreateOptions(firstAlias: 'r'))
            {
                required.IsRequired = true;

                command.Options.Add(required);
            }

            StringWriter writer = new();
            GetHelpBuilder(LargeMaxWidth).Write(command, writer);
            Approvals.Verify(writer.ToString());

            static IEnumerable<Option> CreateOptions(char firstAlias)
            {
                char[] c = Enumerable.Range(firstAlias, 6).Select(x => (char)x).ToArray();

                yield return new Option<string>("option-just-name",
                   new[] { $"--alias-{c[0]}", $"-{c[0]}" });
                yield return new Option<string>("option-name-and-description",
                    new[] { $"--alias-{c[1]}", $"-{c[1]}" },
                    "description6");
                yield return new Option<string>("option-name-and-default-value-string",
                    new[] { $"--alias-{c[2]}", $"-{c[2]}" },
                    argResult => "default7",
                    isDefault: true);
                yield return new Option<FileAccess>(
                    "option-name-and-default-value-enum",
                    new[] { $"--alias-{c[3]}", $"-{c[3]}" },
                    () => FileAccess.Read);
                yield return new Option<string>(
                    "option-name-and-description-and-default-value",
                    new[] { $"--alias-{c[4]}", $"-{c[4]}" },
                    () => "default9",
                    "description9");
                yield return new Option<bool>(
                    "option-and-multi-line-description",
                    new[] { $"--alias-{c[5]}", $"-{c[5]}" },
                    "option-and-\r\nmulti-line\ndescription");
            }
        }
    }
}
