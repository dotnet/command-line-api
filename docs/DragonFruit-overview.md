# What is DragonFruit?

The entry point for a normal .NET console application looks like this:

```csharp
static void Main(string[] args)
{
    Console.WriteLine("Hello World!");
}
```

Interpreting the `string[]` arguments into behaviors has been left as a task for the developer. Did the user ask for help? Did they pass invalid input? Can the input be converted to the types that you need if they're not `string`? These problems are not solved for you.

What if you could declare a strongly-typed `Main` method? This was the question that led to the creation of the experimental app model called "DragonFruit", which allows you to create an entry point with multiple parameters of various types and using default values, like this:

```csharp
static void Main(int intOption = 42, bool boolOption = false, FileInfo fileOption = null)
{
    Console.WriteLine($"The value of intOption is: {intOption}");
    Console.WriteLine($"The value of boolOption is: {boolOption}");
    Console.WriteLine($"The value of fileOption is: {fileOption?.FullName ?? "null"}");
}
```

DragonFruit handles help requests, parsing errors, argument binding, and more for you.

```console
> ./myapp # or: > dotnet run
The value of intOption is: 42
The value of boolOption is: False
The value of fileOption is: null 
```

You don't need to write any special code to get help support.

```console
> ./myapp -h # or: dotnet run -- -h
Usage:
  myapp [options]

Options:
  --int-option     intOption
  --bool-option    boolOption
  --file-option    fileOption
```

If you want more informative help, you can add it using standard XML comments:

```csharp
/// <summary>
/// My example app
/// </summary>
/// <param name="intOption">An option whose argument will bind to an int</param>
/// <param name="boolOption">An option whose argument will bind to a bool</param>
/// <param name="fileOption">An option whose argument will bind to a FileInfo</param>
static void Main(int intOption = 42, bool boolOption = false, FileInfo fileOption = null)
{
    Console.WriteLine($"The value of intOption is: {intOption}");
    Console.WriteLine($"The value of boolOption is: {boolOption}");
    Console.WriteLine($"The value of fileOption is: {fileOption?.FullName ?? "null"}");
}
```

The text of those comments will be shown when a user requests help.

```console
> dotnet run -- -h
Usage:
  myapp [options]

Options:
  --int-option     An option whose argument will bind to an int
  --bool-option    An option whose argument will bind to a bool
  --file-option    An option whose argument will bind to a FileInfo
```

## Arguments

In addition to [options](Syntax-Concepts-and-Parser.md#Options) as shown in the examples above, DragonFruit also supports [arguments](Syntax-Concepts-and-Parser.md#Arguments). By convention, if you name a parameter in the `Main` method `args`, `argument`, or `arguments`, it will be exposed as an argument rather than an option.

```csharp
static void Main(int intOption = 42, string[] args = null)
{
    Console.WriteLine($"The value of intOption is: {intOption}");

    foreach (var arg in args)
    {
        Console.WriteLine(arg);
    }
}
```

```console
> myapp -h
Usage:
  myapp [options] <args>

Arguments:
  <args>

Options:
  --int-option <INT-OPTION>    intOption
  --version                    Display version information
```

The argument follows the same conventions for arity as described in [arguments](Syntax-Concepts-and-Parser.md#Arguments-and-arity).

You can try out DragonFruit by installing the latest preview [package](https://www.nuget.org/packages/System.CommandLine.DragonFruit).

