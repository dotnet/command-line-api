# Building your first app with System.CommandLine.DragonFruit

This walkthrough will show you how to get started using the System.CommandLine.DragonFruit app model to build a command line application.

## Create a new console app

Open a new console and run the following commands:

```console
> dotnet new console -o myApp
> cd myApp
```

## Install the System.CommandLine.DragonFruit package

[![Nuget](https://img.shields.io/nuget/v/System.CommandLine.DragonFruit.svg)](https://nuget.org/packages/System.CommandLine.DragonFruit)

## Add some code

Open `Program.cs`. You'll see that your `Main` method looks like this:

```csharp
static void Main(string[] args)
{
    Console.WriteLine("Hello World!");
}
```

If you run it, you'll see this:

```console
> ./myapp # or: > dotnet run
Hello World!
```

The default main only takes `string` arguments as an array. With DragonFruit, you can accept named arguments of various types and specify default values. Change your `Main` method to this:

```csharp
class Program
{
    /// <param name="intOption">An option whose argument is parsed as an int</param>
    /// <param name="boolOption">An option whose argument is parsed as a bool</param>
    /// <param name="fileOption">An option whose argument is parsed as a FileInfo</param>
    static void Main(int intOption = 42, bool boolOption = false, FileInfo fileOption = null)
    {
        Console.WriteLine($"The value for --int-option is: {intOption}");
        Console.WriteLine($"The value for --bool-option is: {boolOption}");
        Console.WriteLine($"The value for --file-option is: {fileOption?.FullName ?? "null"}");
    }
}
```

You're ready to run your program.

```console
> dotnet run -- --int-option 123
The value for --int-option is: 123
The value for --bool-option is: False
The value for --file-option is: null
```

This program is equivalent to the one demonstrated in [Your first app with System.CommandLine](Your-first-app-with-System-CommandLine.md).

To explore its features, take a look at [Features: overview](Features-overview.md)


## Advanced options

### Async support

`System.CommandLine.DragonFruit` also makes it easy to wire in support for [Process termination handling](Process-termination-handling.md).

Async `Main()` methods are support it and take a `CancellationToken` this cancellation token. Termination of the app triggers the cancellation of that token allowing you to handle interops.

### Untyped arguments

Further more `System.CommandLine.DragonFruit` still allows your `Main()` to accept args for any untyped arguments you wish to (not) handle or pass along.


```csharp
/// <summary>
/// A main function can also take <see cref="CancellationToken"/> which is hooked up to support termination (e.g CTRL+C)
/// </summary>
/// <param name="boolArg"></param>
/// <param name="token"></param>
/// <param name="args"></param>
/// <returns></returns>
private static async Task<int> Main(bool boolArg = false, CancellationToken token = default, string[] args = null)
{
	try
	{
		await Task.Delay(TimeSpan.FromSeconds(20), token);
	}
	catch (OperationCanceledException)
	{
		return 1;
	}
	return 0;
}
```
