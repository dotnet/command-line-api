// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Sorry for temporarily naming this after someone, but it's an attempt at the 
// way he wants to work with System.CommandLine

using System.CommandLine.Invocation;
using System.CommandLine.Binding;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.CommandLine.Builder;

namespace System.CommandLine.Tests.Binding
{
    // Copyright (c) .NET Foundation and contributors. All rights reserved.
    // Licensed under the MIT license. See LICENSE file in the project root for full license information.

    public class DataOnlyUsageTests
    {
        internal class AddData
        {
            public string ProjectName { get; set; }
        }

        // Inheritance gives you access to the ProjectPath
        internal class AddPackageData : AddData
        {
            public string PackageId { get; set; }
            public string Framework { get; set; }
            public bool NoRestore { get; set; }
            public string Source { get; set; }
            public string PackageDirectory { get; set; }
            public bool Interactive { get; set; }
        }

        internal class AddReferenceData : AddData
        {
            public string ProjectPath { get; set; }
            public string Framework { get; set; }
        }



        // OK, I regretted my choice of domain. You might do a "dotnet add -h" to help this make sense
        private Command GetAddAndChildren()
        {
            var dotnetCommand = new Command("dotnet");
            var addCommand = GetAddCommand();
            dotnetCommand.AddCommand(addCommand);
            addCommand.AddCommand(GetPackageCommand());
            addCommand.AddCommand(GetReferenceCommand());
            return dotnetCommand;
        }

        Command GetAddCommand()
        {
            var command = new Command("add");
            command.Argument = new Argument<string>();
            command.Argument.Name = "ProjectName";
            command.Handler = CommandHandler.Create<AddData>();
            return command;
        }

        Command GetPackageCommand()
        {
            var command = new Command("package");
            command.Argument = new Argument<string>();
            command.Argument.Name = "PackageId";
            command.AddOption(
                    new Option(new string[] { "--framework", "-f" },
                        description: "Add the reference only when targeting a specific framework.",
                        argument: new Argument<string>()));
            command.AddOption(
                     new Option(new string[] { "--no-restore", "-n" },
                         description: "Add the reference without performing restore preview and compatibility check.",
                         argument: new Argument<bool>()));
            command.AddOption(
                     new Option(new string[] { "--source", "-s" },
                         description: "The NuGet package source to use during the restore.",
                         argument: new Argument<string>()));
            command.AddOption(
                     new Option(new string[] { "--package-directory" },
                         description: "The directory to restore packages to.",
                         argument: new Argument<string>()));
            command.AddOption(
                     new Option(new string[] { "--interactive", "-i" },
                         description: "Allows the command to stop and wait for user input or action(for example to complete authentication).",
                         argument: new Argument<bool>()));
            command.Handler = CommandHandler.Create<AddPackageData>();
            return command;
        }

        Command GetReferenceCommand()
        {
            var command = new Command("reference");
            command.Argument = new Argument<string>();
            command.Argument.Name = "ProjectPath";
            command.AddOption(
                    new Option(new string[] { "--framework", "-f" },
                        description: "Add the reference only when targeting a specific framework.",
                        argument: new Argument<string>()));
            command.Handler = CommandHandler.Create<AddReferenceData>();
            return command;
        }

        [Fact]
        public void Can_invoke_default_method_on_type_with_parameters()
        {
            const string commandLine = "dotnet add MyProject package Newtonsoft --no-restore --package-directory ./packages";
            AddPackageData addPackageCheck = null;
            AddReferenceData addReferenceCheck = null;

            // Rich, is this the code you are looking for?
            var command = GetAddAndChildren();
            var target = TargetFrromInvocation(command, commandLine);
            command = null;
             switch (target)
            {
                case AddReferenceData addReferenceData:
                    CodeThatAddsReferences(addReferenceData);
                    break;
                case AddPackageData addPackageData:
                    CodeThatAddsPackages(addPackageData);
                    break;
                case AddData oops:
                    // if this is what you want, we need work to ensure you don't get here
                    throw new InvalidOperationException("oops");
            }

            void CodeThatAddsPackages(AddPackageData addPackageData)
                => addPackageCheck = addPackageData;

            void CodeThatAddsReferences(AddReferenceData addReferenceData)
                 => addReferenceCheck = addReferenceData;

            addPackageCheck.Should().NotBeNull();
            addPackageCheck.Interactive.Should().BeFalse();
            addPackageCheck.NoRestore.Should().BeTrue();
            addPackageCheck.PackageId.Should().Be("Newtonsoft");
            addPackageCheck.ProjectName.Should().Be("MyProject");
            addReferenceCheck.Should().BeNull();
        }

        private static object TargetFrromInvocation(Command command, string commandLine)
        {
            var invocationContext = command.MakeDefaultInvocationContext(commandLine);
            var resultCommand = invocationContext.ParseResult.CommandResult.Command;
            var binder = ((resultCommand as Command).Handler as ReflectionCommandHandler).Binder;
            var target = binder.GetTarget(invocationContext);
            return target;
        }
    }
}
