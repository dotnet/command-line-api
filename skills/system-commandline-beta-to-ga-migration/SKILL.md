---
name: system-commandline-beta-to-ga-migration
version: 2.0.0
description: >-
  Use when migrating a .NET CLI from System.CommandLine 2.0.0-beta (beta1–beta4) to the GA / 3.x
  API, or when you see removed beta symbols (SetHandler, AddOption, AddCommand, AddGlobalOption,
  BinderBase<T>, IConsole, HelpBuilder, getDefaultValue:, IsRequired, ExistingOnly). beta4 → GA was a
  breaking redesign of the invocation and binding stack: beta code does NOT compile against GA.
---

# System.CommandLine: 2.0.0-beta → GA / 3.x migration

beta4 → 2.0 GA removed the entire old invocation/binding stack. Beta code does **not** compile against
GA, and most training-data / web snippets are beta. 3.x is additive over GA, so the **same** mapping
carries beta code all the way to 3.x. Set the package version, then apply the table mechanically.

> Do NOT `web_search` / `web_fetch` — the top results are the removed beta API. This table is the
> authoritative rename map.

## Required setup

Set the package version first — beta and GA are the same package id, so the migration starts with
`<PackageReference Include="System.CommandLine" Version="2.0.11" />` — the latest stable — or
`Version="3.0.0-preview.7.26381.103"` for the 3.x preview. The namespace is
unchanged (`using System.CommandLine;`); what changed is the invocation and binding stack.

The GA shape every row below maps onto — declare the instance, add it to a command, read it back by
that instance:

```csharp
using System.CommandLine;

var nameOption = new Option<string>("--name") { Description = "Who to greet" };

var root = new RootCommand("Greeter");
root.Options.Add(nameOption);                          // was root.AddOption(nameOption)
root.SetAction(parseResult =>                          // was root.SetHandler((string n) => ...)
{
    string name = parseResult.GetValue(nameOption)!;   // was a bound delegate parameter
    return 0;
});
return await root.Parse(args).InvokeAsync();           // was root.InvokeAsync(args)
```

## Rename map

| 2.0.0-beta4 | 2.x / 3.x GA |
| ----------- | ------------ |
| `AddOption` / `AddArgument` / `AddCommand` | `Options.Add` / `Arguments.Add` / `Subcommands.Add` |
| `AddGlobalOption(o)` | `o.Recursive = true;` then `Options.Add(o)` |
| `SetHandler(...)` / `Handler.SetHandler(...)` | `command.SetAction(parseResult => ...)` |
| `command.Invoke(args)` / `InvokeAsync(args)` | `command.Parse(args).Invoke()` / `.InvokeAsync()` |
| handler params bound by position | `parseResult.GetValue(option)` inside the action |
| `getDefaultValue: () => v` ctor arg / `SetDefaultValue` / `SetDefaultValueFactory` | `DefaultValueFactory = _ => v` |
| `IsRequired` | `Required` |
| `ExistingOnly()` | `AcceptExistingOnly()` |
| `ArgumentHelpName` | `HelpName` |
| binding: `BinderBase<T>` / `BindingContext` / `IValueDescriptor` | `parseResult.GetValue(option)` |
| `IConsole` / `HelpBuilder` | removed; use `Console`, customize help via `HelpAction` |

## Before (beta4) → After (GA/3.x)

```csharp
// BETA4 — does NOT compile against GA
var name = new Option<string>("--name", getDefaultValue: () => "world", description: "Name") { IsRequired = true };
var root = new RootCommand("Greeter");
root.AddOption(name);
root.SetHandler((string n) => Console.WriteLine($"Hello {n}"), name);
return await root.InvokeAsync(args);
```

```csharp
// GA / 3.x
var name = new Option<string>("--name")                 // NOT ("--name","Name") — 2nd string is an alias
{
    Description = "Name",
    Required = true,
    DefaultValueFactory = _ => "world",
};
var root = new RootCommand("Greeter");
root.Options.Add(name);
root.SetAction(parseResult => Console.WriteLine($"Hello {parseResult.GetValue(name)}"));
return await root.Parse(args).InvokeAsync();
```

## Upgrading 2.x → 3.x (NOT beta)

Drop-in: bump the version, change **no** code (3.x is additive; no API breaks). Do **not**
"modernize" working 2.x patterns. The only consumer-visible shift is the dropped in-box `net8.0`
target — the package now targets `net10.0` and `netstandard2.0`. The members 3.x adds over 2.x are covered separately.

## The silent trap while migrating

`new Option<T>("--name", "description")` still **compiles** — the 2nd positional arg became an alias in
beta4. So this migrates cleanly-looking but registers your description as a bogus alias and loses the
help text. Always move descriptions to `{ Description = "..." }`; extra ctor strings are always aliases.
