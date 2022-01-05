# Building your first app with System.CommandLine

This walkthrough will show you how to get started using System.CommandLine to build a command line application.

## Create a new console app

Open a new console and run the following commands in an empty directory:

```console
> dotnet new console -o myApp
> cd myApp
```

## Install the System.CommandLine package

Use `dotnet` to add the package to your project. In the project directory, run:

```console
> dotnet add package System.CommandLine --prerelease
```

## Add some code

Open `Program.cs`. At the top, add a `using` directive:

```csharp
using System.CommandLine;
```

`Program.cs` contains the following code:

```csharp
Console.WriteLine("Hello World!");
```

This isn't doing anything with the `args` parameter. For that, we'll use System.CommandLine.

At the top of the file, add the following `using` directives:

```csharp
using System.CommandLine;
using System.IO;
```

Now change your `Main` method to this:

```csharp
// Create some options:
var intOption = new Option<int>(
        "--int-option",
        getDefaultValue: () => 42,
        description: "An option whose argument is parsed as an int");
var boolOption = new Option<bool>(
        "--bool-option",
        "An option whose argument is parsed as a bool");
var fileOption = new Option<FileInfo>(
        "--file-option",
        "An option whose argument is parsed as a FileInfo");

// Add the options to a root command:
var rootCommand = new RootCommand
{
    intOption,
    boolOption,
    fileOption
};

rootCommand.Description = "My sample app";

rootCommand.SetHandler((int i, bool b, FileInfo f) =>
{
    Console.WriteLine($"The value for --int-option is: {i}");
    Console.WriteLine($"The value for --bool-option is: {b}");
    Console.WriteLine($"The value for --file-option is: {f?.FullName ?? "null"}");
}, intOption, boolOption, fileOption);

// Parse the incoming args and invoke the handler
return rootCommand.Invoke(args);
```

You're ready to run your program.

```console
> dotnet run -- --int-option 123
The value for --int-option is: 123
The value for --bool-option is: False
The value for --file-option is: null
```

This program is equivalent to the one demonstrated in [Your first app with System.CommandLine.DragonFruit](Your-first-app-with-System-CommandLine-DragonFruit.md).

To explore its features, take a look at [Features: overview](Features-overview.md)
