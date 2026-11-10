# System.CommandLine.StaticCompletions

System.CommandLine.StaticCompletions generates *static* shell completion scripts from a [System.CommandLine](https://www.nuget.org/packages/System.CommandLine) `Command` tree. It walks your command hierarchy - commands, subcommands, options, arguments, and aliases - and emits a native completion script for the user's shell.

These scripts are "static" because they describe the shape of your CLI ahead of time and can be shipped, checked in, or installed by a package manager. Unlike the default `dotnet-suggest` integration, they don't require a separate global tool to be installed to provide completions for the fixed parts of your CLI. Where you genuinely need runtime values (for example, completing a name looked up from a database), individual symbols can opt into *dynamic* completions that call back into your application.

Supported shells:

* Bash (`bash`)
* Zsh (`zsh`)
* PowerShell / PowerShell Core (`pwsh`)
* Fish (`fish`)
* Nushell (`nushell`)

## Getting Started

Add a `completions` command to your application's root command. The `CompletionsCommandDefinition` provides the `completions script <shell>` command, and `CompletionsCommandParser.ConfigureCommand` wires up the action that produces the script:

```csharp
using System.CommandLine;
using System.CommandLine.StaticCompletions;

RootCommand rootCommand = new("My application");

// ... add your options, arguments, and subcommands ...

// Add the `completions` command to your CLI.
CompletionsCommandDefinition completionsCommand = new();
CompletionsCommandParser.ConfigureCommand(completionsCommand);
rootCommand.Subcommands.Add(completionsCommand);

return rootCommand.Parse(args).Invoke();
```

Your users can now generate a completion script for their shell:

```shell
# Generate a script for a specific shell
> myapp completions script bash > ~/.local/share/bash-completion/completions/myapp

# The shell argument is optional - it defaults to the current shell
> myapp completions script >> $PROFILE
```

When the shell argument is omitted, the shell is detected from the environment (the `SHELL` variable on Unix, PowerShell on Windows).

## Dynamic Completions

Static scripts are perfect for the fixed structure of your CLI, but some completions can only be computed at runtime. Mark an option or argument as dynamic and the generated script will call back into your application (via System.CommandLine's built-in `[suggest]` directive) to resolve those values on demand:

```csharp
using System.CommandLine;
using System.CommandLine.StaticCompletions;

Option<string> nameOption = new("--name")
{
    Description = "A name resolved at completion time"
};

// Values will be resolved by invoking the app at completion time.
nameOption.IsDynamic = true;
nameOption.CompletionSources.Add(_ => GetNamesFromDatabase());

rootCommand.Options.Add(nameOption);
```

> **Note:** Dynamic completions rely on the `[suggest]` directive being present on your root command (it is enabled by default). If you have removed it while dynamic symbols are present, script generation will emit a warning because those completions would silently do nothing.

## Framework Support

* **.NET 8.0+** - Trimming and AOT compatible.

## License

This package is licensed under the [MIT License](https://opensource.org/licenses/MIT).

## Documentation

* **[Microsoft Learn Documentation](https://learn.microsoft.com/dotnet/standard/commandline/)** - Guides and API reference for System.CommandLine.
* **[GitHub Repository](https://github.com/dotnet/command-line-api)** - Source code, samples, and issues.

## Support

* **Issues**: [GitHub Issues](https://github.com/dotnet/command-line-api/issues)
* **Discussions**: [GitHub Discussions](https://github.com/dotnet/command-line-api/discussions)
