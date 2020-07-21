# History

The new `System.CommandLine`'s lineage can be traced through the experiences of various teams at Microsoft building .NET command line applications. Some of its ideas have been expressed in the `dotnet` CLI  and `CommandLineUtils` originally created in ASP.NET with a [popular fork](https://github.com/natemcmaster/CommandLineUtils) by @natemcmaster.

This project is called the "new" `System.CommandLine` because Microsoft also had a `System.CommandLine` project in CoreFxLab that has been retired because it did not meet the current needs.

`System.CommandLine` is not a "Microsoft project." It is a community project with both Microsoft employees and community members as contributors. We've been working with folks on the CoreFx team so that we are aligned for inclusion in .NET Core if `System.CommandLine` meets the quality bar of the community and Microsoft. This relationship also allows us to work within the `dotnet` organization on GitHub. A few teams at Microsft have been early adopters, avoiding one-off parsers and trying to be consistent across `dotnet` CLI experiences.

The addition of global tools to the `dotnet` ecosystem meant the creation of more mini-CLIs. It is quite easy to set out to build a global tool and then spend more time building its CLI than on writing the code for the actual problem you were trying to solve. And if you aren't forward-looking, you can easily create a system where the functionality of the tool and the command line parsing cannot be separately tested.

Parsing is a deceptively complex problem. With folks on the team that have been involved in writing complex and successful CLIs, we can work in the other direction, focusing first on a solid core and then working outward toward the functionality and API surface. We're building the foundation that the folks who wrote and worked on the `dotnet` CLI wish they had been able to use. We ran this part of the project in private to ensure the core was solid, and as an all-volunteer effort this phase took painfully long.

The next layer, the API over the core functionality, has undergone a rewrite, and we are fairly happy with how you explicitly define your CLI. Explicitly defining a syntax feels like too much work for many scenarios and we've put a lot of thought and discussion into the idea of "app models" that infer the structure of your CLI from something that is natural to write.

App models have two aspects: defining the CLI's structure and bridging between parse results and your application.

As an example, one approach is to bind to a method, which is executed with parameters matching the options and arguments your user provided. This method binding works well when mapped one-to-one between the command and the invoked method. You can find this as the basis for the experimental "DragonFruit" model. DragonFruit takes this idea one step furher by wrapping your `Program.Main` entry point and just letting your `Main` method have normal parameters that just happen to be able to be of types other than `string`. We have not yet found a way we like to do subcommands with the DragonFruit model.

Other approaches to binding include binding to classes. A fundamental aspect of `System.CommandLine` is top-level support for app models and binders. These may reside outside `System.CommandLine` and we hope it will be a framework that people with other ideas can build on. The goal is that app models will interoperate with one another allowing many choices for programmers.

## Rendering

We've also done preliminary work on rendering. This is another area where `System.Console` is showing its age. In particular, `System.Console` does not supporting ANSI terminals easily. While `ncurses` has long addressed differing terminal capabilities in the non-Windows world, .NET programmers could benefit from something that works consistently on Windows, Linux, and Mac.

And with the new Windows Terminal and Windows 10 bringing [virtual terminal capabilities](https://blogs.msdn.microsoft.com/commandline/2018/06/27/windows-command-line-the-evolution-of-the-windows-command-line/) to the Windows console, we think there's an opportunity for .NET APIs that these benefits to users.

