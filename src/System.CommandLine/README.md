# System.CommandLine

System.CommandLine provides robust support for command-line parsing, invocation, and shell completions in .NET applications. It supports both POSIX and Windows conventions, making it easy to build professional command-line interfaces.

## Getting Started

### Basic Command

Here's a simple "Hello World" command-line application:

```csharp
using System.CommandLine;

RootCommand rootCommand = new("Sample command-line app");

Option<string> nameOption = new("--name", "-n")
{
    Description = "Your name"
};

rootCommand.Options.Add(nameOption);

rootCommand.SetAction(parseResult =>
{
    string name = parseResult.GetValue(nameOption);
    Console.WriteLine($"Hello, {name ?? "World"}!");
});

return rootCommand.Parse(args).Invoke();
```

In this example, we create a `RootCommand`, add an option for the user's name, and define an action that prints a greeting. The `RootCommand` is a special kind of `Command` that comes with a few predefined behaviors:

* It discovers its name automatically from the currently-running application
* It automatically provides `--help` and `--version` options and default behaviors for them
* It provides a default integration with `dotnet-suggest` for dynamic [shell completions](#shell-completions)

You can always override or customize these behaviors as needed on a `RootCommand`, or create your own top-level `Command` instead.

### Commands with Arguments

Arguments are values passed directly to commands without option names:

```csharp
var fileArgument = new Argument<FileInfo>("file")
{
    Description = "The file to process"
};

var processCommand = new Command("process", "Process a file");
processCommand.Arguments.Add(fileArgument);

processCommand.SetAction(parseResult =>
{
    FileInfo file = parseResult.GetValue(fileArgument);
    Console.WriteLine($"Processing {file.FullName}");
});

var rootCommand = new RootCommand();

rootCommand.Subcommands.Add(processCommand);
```

### Options with Default Values

Options can have default values and validation:

```csharp
var rootCommand = new RootCommand();

var delayOption = new Option<int>("--delay", "-d")
{
    Description = "Delay in milliseconds",
    DefaultValueFactory = _ => 1000
};

delayOption.Validators.Add(result =>
{
    if (result.GetValueOrDefault<int>() < 0)
    {
        result.AddError("Delay must be non-negative");
    }
});

rootCommand.Options.Add(delayOption);
```

### Subcommands

Build complex CLI applications with nested commands:

```csharp
var rootCommand = new RootCommand("My application");

var configCommand = new Command("config", "Configure the application");
var configSetCommand = new Command("set", "Set a configuration value");
var configGetCommand = new Command("get", "Get a configuration value");

var keyOption = new Option<string>("--key")
{
    Description = "Configuration key"
};
var valueOption = new Option<string>("--value")
{
    Description = "Configuration value"
};

configSetCommand.Options.Add(keyOption);
configSetCommand.Options.Add(valueOption);
configGetCommand.Options.Add(keyOption);

configCommand.Subcommands.Add(configSetCommand);
configCommand.Subcommands.Add(configGetCommand);
rootCommand.Subcommands.Add(configCommand);

// Usage: myapp config set --key "apiUrl" --value "https://api.example.com"
// Usage: myapp config get --key "apiUrl"
```

### Using Options in Command Actions

Access option values through the ParseResult:

```csharp
var connectionOption = new Option<string>("--connection")
{
    Description = "Database connection string"
};
var timeoutOption = new Option<int>("--timeout")
{
    Description = "Timeout in seconds",
    DefaultValueFactory = _ => 30
};
var verboseOption = new Option<bool>("--verbose")
{
    Description = "Enable verbose output"
};

rootCommand.Options.Add(connectionOption);
rootCommand.Options.Add(timeoutOption);
rootCommand.Options.Add(verboseOption);

rootCommand.SetAction(parseResult =>
{
    var connection = parseResult.GetValue(connectionOption);
    var timeout = parseResult.GetValue(timeoutOption);
    var verbose = parseResult.GetValue(verboseOption);
    
    Console.WriteLine($"Connection: {connection}");
    Console.WriteLine($"Timeout: {timeout}");
    Console.WriteLine($"Verbose: {verbose}");
});
```

### Shell Completions

Enable tab completion for your CLI:

```csharp
// Completions are automatically available for all commands, options, and arguments
var rootCommand = new RootCommand("My app with completions");

var fileOption = new Option<FileInfo>("--file")
{
    Description = "The file to process"
};

// Add custom completions using CompletionSources
fileOption.CompletionSources.Add(ctx =>
    // hard-coded list of files
    ["file1.txt", "file2.txt", "file3.txt" ]
);

// Or add simple string suggestions
fileOption.CompletionSources.Add("option1", "option2", "option3");

rootCommand.Options.Add(fileOption);
```

Users can then easily trigger your completions using `dotnet-suggest`:

```shell
> dotnet tool install -g dotnet-suggest
> dotnet suggest script bash > ~/.bashrc
```

```powershell
> dotnet tool install -g dotnet-suggest
> dotnet suggest script powershell >> $PROFILE
```

Once `dotnet-suggest` is installed, you can register your app with it for completions support:

```shell
> dotnet-suggest register --command-path /path/to/myapp
```

Alternatively, you can create your own commands for completion generation and instruct users on how to set them up.

### Async Command Handlers

Support for asynchronous operations:

```csharp
var urlOption = new Option<string>("--url")
{
    Description = "The URL to fetch"
};
rootCommand.Options.Add(urlOption);

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var url = parseResult.GetValue(urlOption);
    if (url != null)
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(url, cancellationToken);
        Console.WriteLine(response);
    }
});

// Or return an exit code:
rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    // Your async logic here
    return await Task.FromResult(0); // Return exit code
});
```

## Notable Changes Since v2.0.0-beta7

### New Features
- **Improved Help System**: Enhanced `HelpAction` to allow users to provide custom `MaxWidth` for help text formatting ([#2635](https://github.com/dotnet/command-line-api/pull/2635)). Note that if you create custom Help or Version actions, you'll want to set `ClearsParseErrors` to `true` to ensure that invoking those features isn't treated like an error by the parser.
- **`Task<int>` Support**: Added `SetAction` overload for `Task<int>` return types ([#2634](https://github.com/dotnet/command-line-api/issues/2634))
- **Detect Implicit Arguments**: Added the `ArgumentResult.Implicit` property for better argument handling ([#2622](https://github.com/dotnet/command-line-api/issues/2622), [#2625](https://github.com/dotnet/command-line-api/pull/2625))
- **Performance Improvements**: Reduced reflection usage throughout the library for better performance ([#2662](https://github.com/dotnet/command-line-api/pull/2662))

### Bug Fixes
- Fixed issue [#2128](https://github.com/dotnet/command-line-api/issues/2128): Resolved command parsing edge cases ([#2656](https://github.com/dotnet/command-line-api/pull/2656))
- Fixed issue [#2257](https://github.com/dotnet/command-line-api/issues/2257): Corrected argument validation behavior
- Fixed issue [#2589](https://github.com/dotnet/command-line-api/issues/2589): Improved error message clarity ([#2654](https://github.com/dotnet/command-line-api/pull/2654))
- Fixed issue [#2591](https://github.com/dotnet/command-line-api/issues/2591): Resolved option parsing inconsistencies ([#2644](https://github.com/dotnet/command-line-api/pull/2644))
- Fixed issue [#2622](https://github.com/dotnet/command-line-api/issues/2622): Enhanced implicit argument support ([#2625](https://github.com/dotnet/command-line-api/pull/2625))
- Fixed issue [#2628](https://github.com/dotnet/command-line-api/issues/2628): Corrected help text formatting issues
- Fixed issue [#2634](https://github.com/dotnet/command-line-api/issues/2634): Added missing Task<int> action support
- Fixed issue [#2640](https://github.com/dotnet/command-line-api/issues/2640): Resolved completion suggestions for nested commands ([#2646](https://github.com/dotnet/command-line-api/pull/2646))

### Breaking Changes
- Default value handling for `ProcessTerminationTimeout` has been re-added ([#2672](https://github.com/dotnet/command-line-api/pull/2672))
- Some internal APIs have been refactored to reduce reflection usage ([#2662](https://github.com/dotnet/command-line-api/pull/2662))

### Other Improvements
- Updated to .NET 10.0 RC1 compatibility
- Improved memory usage and performance optimizations
- Better handling of complex command hierarchies

## Documentation

For comprehensive documentation, tutorials, and API reference, visit:
- **[Microsoft Learn Documentation](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)** - Complete guides and API reference
- **[GitHub Repository](https://github.com/dotnet/command-line-api)** - Source code, samples, and issues

## Framework Support

- **.NET 8.0+** - Full feature support with trimming and AOT compilation
- **.NET Standard 2.0** - Compatible with .NET Framework 4.6.1+, .NET Core 2.0+

## License

This package is licensed under the [MIT License](https://opensource.org/licenses/MIT).

## Contributing

We welcome contributions! Please see our [Contributing Guide](https://github.com/dotnet/command-line-api/blob/main/CONTRIBUTING.md) for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/dotnet/command-line-api/issues)
- **Discussions**: [GitHub Discussions](https://github.com/dotnet/command-line-api/discussions)