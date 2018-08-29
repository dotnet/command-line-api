using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Tests.kdollard.ClassDef
{
    [Command]
    public class Dotnet
    {
    }

    [Command]
    [CommandDefault]
    public class Project
    { }
    new CommandLine("dotnet" )
                    .AddCommand("project", isDefault: true,
                       subCommands: new Command[] {
                            new Command(
                                    "add",
                                    new Argument<string>("projectName", Arity.ExactlyOne))
                                    .AddCommand("package", argument: new Argument<string>("packagId", Arity.ExactlyOne)),
                            new Command(
                                    "remove",
                                    new Argument<string>("projectName", Arity.ExactlyOne))
                                    .AddCommand("reference", argument: new Argument<string>("project2projectPath", Arity.ExactlyOne)),
                            new Command("list")
                                })
                   .AddCommand("tool", isDefault: true,
                       subCommands: new Command[] {
                            new Command("install")
                                    .AddOption("global","g")
                                    .AddOption("tool-path", argument: new Argument<string>("toolPath"))
                                    .AddOption("version", argument: new Argument<string>("version", Arity.ExactlyOne))
                                    .AddOption("configfile", argument: new Argument<string>("configfile", Arity.ExactlyOne))
                                    .AddOption("source-feed", argument: new Argument<string>("source-feed", Arity.ExactlyOne))
                                    .AddOption("framework", argument: new Argument<string>("framework", Arity.ExactlyOne))
                                    .AddOption("verbosity", "v", argument: new Argument<string>("verbosity", Arity.ExactlyOne)),
                            new Command("uninstall")
                                    .AddOption("global", "g")
                                    .AddOption("tool-path", argument: new Argument<string>("toolPath", Arity.ExactlyOne)),
                            new Command("update")
                                    .AddOption("global", "g")
                                    .AddOption("tool-path", argument: new Argument<string>("toolPath", Arity.ExactlyOne)),
                           new Command("list")
                                    .AddOption("global", "g")
                                    .AddOption("tool-path", argument: new Argument<string>("toolPath", Arity.ExactlyOne)),
                                });  }
}
