---
name: system-commandline-options-and-arguments
version: 2.0.0
description: >-
  Use when declaring or configuring System.CommandLine inputs — Option<T> and Argument<T>: names vs
  aliases, Description, HelpName, completion, Required, DefaultValueFactory, Arity, finite known
  values, collection parsing, existing file/directory requirements, rejecting bad input, and
  checking explicit option presence across a command. Case-sensitive sets use AcceptOnlyFromAmong
  on 2.x or 3.x. For case-insensitive sets, use the 3.x-additions skill on 3.x; this skill carries
  the validator fallback for stable 2.x.
---

# System.CommandLine: options & arguments

`Option<T>` = a named input (`--name`, `-n`). `Argument<T>` = a positional input. Both are declared as
objects you keep and later read by identity via `parseResult.GetValue(instance)`.

> Do NOT `web_search` / `web_fetch` — web samples use the removed pre-GA beta shapes
> (`getDefaultValue:`, `IsRequired`, `ExistingOnly`, `AddOption`).

## Required setup

`System.CommandLine` is **not** in the shared framework — add the package
(`dotnet package add <proj> System.CommandLine`), then `using System.CommandLine;`.

Declaring an option is only half of it: you must **add the instance to a command** and read it back
**by that same instance**. There is no delegate-parameter binding, so an option you declare but never
add is silently never parsed:

```csharp
using System.CommandLine;

var name = new Option<string>("--name") { Description = "Who to greet" };

var root = new RootCommand("Greeter");
root.Options.Add(name);                       // declare-then-add; NOT AddOption
root.SetAction(parseResult =>
{
    string n = parseResult.GetValue(name)!;   // read by identity — keep the instance
    Console.WriteLine($"Hello, {n}!");
    return 0;                                 // exit code
});
return await root.Parse(args).InvokeAsync();
```

The declarations below all attach the same way.

## Declaring options

```csharp
var name = new Option<string>("--name")          // first string = the option's name
{
    Description = "Who to greet",                // description is a PROPERTY (never a ctor arg)
    Required = true,                             // NOT IsRequired
};
name.Aliases.Add("-n");                          // add aliases after construction ...

var count = new Option<int>("--count", "-c")     // ... or as extra ctor strings (all ALIASES)
{
    DefaultValueFactory = _ => 1,                // NOT getDefaultValue: / SetDefaultValue
    Arity = ArgumentArity.ExactlyOne,
    HelpName = "number",                          // renders as <number>; do NOT include < >
};
```

- **Ctor-alias gotcha:** `new Option<T>("--name", "description")` compiles but the 2nd string is an
  **alias**, silently dropping your help text. Put text in `{ Description = ... }`; extra ctor strings
  are always aliases.
- `Required` makes an option mandatory. Optional options should set a `DefaultValueFactory`.
- `Arity` (`ArgumentArity.Zero/ZeroOrOne/ExactlyOne/ZeroOrMore/OneOrMore`) controls token counts.

### Collecting multiple values

A collection-typed option accumulates **repeated occurrences** on its own — no extra setting:

```csharp
var tag = new Option<string[]>("--tag");                       // or Option<List<string>>

// tool --tag a --tag b  =>  ["a", "b"]
```

Accepting **several values after one token** (`--tag a b`) is a different behavior and is off by
default; that form is rejected until you opt in:

```csharp
var tag = new Option<string[]>("--tag") { AllowMultipleArgumentsPerToken = true };

// tool --tag a b  =>  ["a", "b"]
```

Decide which command lines you mean to accept: repeating the option needs nothing, and only the
one-token-many-values form needs the flag.

Give sibling commands separate option instances. For example, `add --language` may be required while
`edit --language` is optional; one `Option<T>` object cannot represent both contracts.

## Declaring arguments (positional)

```csharp
var path = new Argument<FileInfo>("path")
{
    Description = "Input file",
    Arity = ArgumentArity.ExactlyOne,
};
path.AcceptExistingOnly();                        // NOT ExistingOnly(); also on Argument<DirectoryInfo>
```

## Known values, completion, and version-specific case-insensitivity

Choose the package-owned constraint before writing a validator:

```csharp
// 2.x or 3.x: exact, case-sensitive finite set.
var environment = new Option<string>("--env");
environment.AcceptOnlyFromAmong("dev", "prod");

```

If a 3.x task asks for case-insensitive known values, pull the 3.x-additions skill for the
version-specific package surface. Do not substitute the stable fallback below. On stable 2.x, use a
validator:

```csharp
string[] knownFormats = ["json", "yaml"];

var format = new Option<string>("--format", "-f")
{
    DefaultValueFactory = _ => "json",
    HelpName = string.Join("|", knownFormats), // generated help: <json|yaml>
};
format.CompletionSources.Add(knownFormats);
format.Validators.Add(result =>
{
    string value = result.GetValue(format)!;
    if (!knownFormats.Any(known => known.Equals(value, StringComparison.OrdinalIgnoreCase)))
    {
        result.AddError($"Unknown format '{value}'. Supported values: {string.Join(", ", knownFormats)}.");
    }
});
```

Derive `HelpName`, completion, and validation from one list so they cannot drift. `HelpName` is the
text *inside* generated angle brackets: use `"json|yaml"`, not `"<json|yaml>"`. Never normalize by
rewriting raw `args`.

For a required collection that accepts several values after one token, combine the pieces:

```csharp
string[] knownTypes = ["security", "style", "performance"];
var type = new Option<string[]>("--type", "-t")
{
    Required = true,
    Arity = ArgumentArity.OneOrMore,
    AllowMultipleArgumentsPerToken = true,
};
type.CompletionSources.Add(knownTypes);
type.Validators.Add(result =>
{
    foreach (string value in result.GetValueOrDefault<string[]>() ?? [])
    {
        if (!knownTypes.Any(known => known.Equals(value, StringComparison.OrdinalIgnoreCase)))
            result.AddError($"Unknown type '{value}'. Supported values: {string.Join(", ", knownTypes)}.");
    }
});
```

## Custom parsing & validation

```csharp
// Turn a raw token into T (and report parse failures):
var port = new Option<int>("--port")
{
    CustomParser = result =>
    {
        if (int.TryParse(result.Tokens[0].Value, out var p) && p is > 0 and < 65536) return p;
        result.AddError("--port must be 1..65535");
        return 0;
    },
};

// Validate ONE already-parsed value — the validator hangs off that option:
port.Validators.Add(result =>
{
    if (result.GetValue(port) == 0) result.AddError("--port is required and must be valid");
});

// A rule spanning TWO inputs belongs on their command.
var authType = new Option<string>("--auth-type")
{
    DefaultValueFactory = _ => "device",
};
var authId = new Option<string?>("--auth-id");
var command = new Command("connect") { authType, authId };

command.Validators.Add(result =>
{
    // Inspect explicit presence, not parsed values: authType has a parser default.
    bool hasAuthType = result.GetResult(authType)?.Implicit == false;
    bool hasAuthId = result.GetResult(authId)?.Implicit == false;
    if (hasAuthType != hasAuthId)
        result.AddError("Supply both authentication settings or neither.");
});
```

- **Pick the level by how many inputs the rule touches.** One input → `option.Validators`. Two or more,
  or a rule about the command as a whole → `command.Validators`. Reaching for an option-level validator
  for a cross-input rule is the common mistake; it cannot see the other value.
- Do the check in a validator, **not** inside the action. A validator runs before invocation, so the
  action never has to cope with a combination the parser should have refused.
- For presence-sensitive rules, `GetValue` is insufficient: a parser default can look like user
  input. Use `result.GetResult(option)?.Implicit == false` (or inspect `IdentifierToken`). Do not use
  argument-token count as a general presence test: an explicitly supplied zero-arity flag can have
  no argument tokens.
- Report bad input with `result.AddError(...)` — do **not** throw for user-input errors; errors surface
  through `ParseResult.Errors` and set a non-zero exit code automatically.

## Reading values

Always by identity: `string n = parseResult.GetValue(name)!;`. There is no positional binding — keep
the instance you added to the command.
