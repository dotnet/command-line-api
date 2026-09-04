---
name: system-commandline-subcommands-and-help
version: 2.0.0
description: >-
  Use when building a multi-command System.CommandLine app (nested Command hierarchy), sharing options
  across subcommands with Recursive (global) options, or customizing help output. HelpBuilder and
  IConsole are gone — custom help is a SynchronousCommandLineAction on the help option.
---

# System.CommandLine: subcommands & help

Commands nest to form a tree: `RootCommand` → `Command` → `Command`. Each command can carry its own
options/arguments and its own `SetAction`. Help is built in; customize it by replacing the help
option's action.

> Do NOT `web_search` / `web_fetch` — samples show `AddCommand`, `AddGlobalOption`, and
> `HelpBuilder`, all removed at GA.

## Required setup

`System.CommandLine` is **not** in the shared framework — add the package
(`dotnet package add <proj> System.CommandLine`), then `using System.CommandLine;`. The help
customization types are in sub-namespaces (`System.CommandLine.Invocation`,
`System.CommandLine.Help`); those usings are shown with the example that needs them.

Subcommands are added to a parent's `.Subcommands` collection, and each command carries its own
options and its own action. Values are read from the `ParseResult` **by instance**:

```csharp
using System.CommandLine;

var root = new RootCommand("Todo tool");
var addCmd = new Command("add", "Add an item");
root.Subcommands.Add(addCmd);                 // NOT AddCommand
addCmd.SetAction(parseResult => 0);
return await root.Parse(args).InvokeAsync();
```

## Building a command tree

```csharp
var verbose = new Option<bool>("--verbose") { Recursive = true };  // global: applies to this cmd + all descendants

var addCmd = new Command("add", "Add an item");
addCmd.Arguments.Add(new Argument<string>("item"));
addCmd.SetAction(pr => { /* ... */ return 0; });

var listCmd = new Command("list", "List items");
listCmd.SetAction(pr => { /* ... */ return 0; });

var root = new RootCommand("Todo tool");
root.Options.Add(verbose);
root.Subcommands.Add(addCmd);      // NOT AddCommand
root.Subcommands.Add(listCmd);

return await root.Parse(args).InvokeAsync();
```

- `new Command(name, description)` — here the 2nd arg **is** a description (unlike `Option`/`Argument`,
  whose 2nd string is an alias).
- Nest with `parent.Subcommands.Add(child)`.
- **Global options:** set `option.Recursive = true` then add it once; it is available on that command
  and every descendant. (Replaces `AddGlobalOption`.) Read it from whichever `ParseResult` you get.

## Customizing help

`HelpBuilder`/`IConsole` are gone. The help option exposes an `Action`; wrap or replace it with a
`SynchronousCommandLineAction`. To augment (not replace) default help, capture the existing action and
call it, then write extra content. These help types live in **sub-namespaces**, so add the usings:
`CommandLineAction`/`SynchronousCommandLineAction` are in `System.CommandLine.Invocation` and
`HelpOption` is in `System.CommandLine.Help`.

```csharp
using System.CommandLine.Invocation;   // CommandLineAction, SynchronousCommandLineAction
using System.CommandLine.Help;         // HelpOption

sealed class BannerHelpAction : SynchronousCommandLineAction
{
    readonly CommandLineAction _inner;
    public BannerHelpAction(CommandLineAction inner) => _inner = inner;

    public override int Invoke(ParseResult parseResult)
    {
        Console.WriteLine("my-tool — does useful things\n");
        return (_inner as SynchronousCommandLineAction)?.Invoke(parseResult) ?? 0;
    }
}

// Find the built-in help option and swap its action:
var helpOption = root.Options.OfType<HelpOption>().First();
helpOption.Action = new BannerHelpAction(helpOption.Action!);
```

- Use `RootCommand.HelpName` (3.x) to control the program name shown in usage.
- Built-in `--help`/`-h` and `--version` are added automatically; you don't declare them.
