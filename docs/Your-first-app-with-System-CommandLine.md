# Building your first app with System.CommandLine

This walkthrough will show you how to get started using System.CommandLine to build a command line application.

## Create a new console app

Open a new console and run the following commands:

```console
> dotnet new console -o myApp
> cd myApp
```

## Install the System.CommandLine package

Use `dotnet` to add the package to your project. From the project directory, run:

```console
> dotnet add package System.CommandLine --prerelease
```

Or see more options on Nuget
[![Nuget](https://img.shields.io/nuget/v/System.CommandLine.svg)](https://nuget.org/packages/System.CommandLine)

## Add some code

Open `Program.cs`. At the top, add a `using` directive:

```csharp
using System.CommandLine;
```

Your `Main` method looks like this:

```csharp
static void Main(string[] args)
{
    Console.WriteLine("Hello World!");
}
```

Now, let's add a parser.

You'll need a few more `using` directives:

```csharp
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
```

Now change your `Main` method to this:

```csharp
static int Main(string[] args)
{
    // Create a root command with some options
    var rootCommand = new RootCommand
    {
        new Option<int>(
            "--int-option",
            getDefaultValue: () => 42,
            description: "An option whose argument is parsed as an int"),
        new Option<bool>(
            "--bool-option",
            "An option whose argument is parsed as a bool"),
        new Option<FileInfo>(
            "--file-option",
            "An option whose argument is parsed as a FileInfo")
    };

    rootCommand.Description = "My sample app";

   // Note that the parameters of the handler method are matched according to the names of the options
   rootCommand.Handler = CommandHandler.Create<int, bool, FileInfo>((intOption, boolOption, fileOption) =>
    {
        Console.WriteLine($"The value for --int-option is: {intOption}");
        Console.WriteLine($"The value for --bool-option is: {boolOption}");
        Console.WriteLine($"The value for --file-option is: {fileOption?.FullName ?? "null"}");
    });

    // Parse the incoming args and invoke the handler
    return rootCommand.InvokeAsync(args).Result;
}
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
