# Model Binding

_**This document describes System.CommandLine Beta 1. Some of the functionality described here now requires a separate package, [System.CommandLine.NamingConventionBinder](https://www.nuget.org/packages/System.CommandLine.NamingConventionBinder). For more details, please see [the Beta 2 announcement](https://github.com/dotnet/command-line-api/issues/1537).**_

Parsing command line arguments is a means to an end. You probably don't really want to think about parsing the command line. You just want some arguments passed to a method, based on some command line arguments.

In C#, the application entry point method has always looked something like this:

```cs
static void Main(string[] args)
{ 
}
```

The goal of every command line parsing library is to turn the string array passed to `Main` into something more useful. You might ultimately want to call a method that looks like this:

```cs
void Handle(int anInt)
{
}
```

So for example, you might want an input `123` from the command line to be converted into an `int` with the value `123`. This conversion of command line input into variables or arguments that you can use in your code is called "binding." The term "model binding" refers to binding simple types as well as more complex types in order to pass the values to a method.

# Binding parameters to a command handler

The simplest way to bind command line input is to set the `Handler` property on a `Command`. The `System.CommandLine` model binder will look at the options and arguments for the command and attempt to match them to the parameters of the specified handler method. The default convention is that parameters are matched by name, so in the following example, option `--an-int` matches the parameter named `anInt`. Matching ignores hyphens (and other option prefixes, such as `'/'`) and is case insensitive.

``` cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region MultipleArgs --session MultipleArgs
var command = new RootCommand
            {
                new Option<string>("--a-string"),
                new Option<int>("--an-int")
            };

command.Handler = CommandHandler.Create(
    (string aString, int anInt) =>
    {
        Console.WriteLine(aString);
        Console.WriteLine(anInt);
    });

await command.InvokeAsync("--an-int 123 --a-string \"Hello world!\" ");
```

``` console --session MultipleArgs
Hello world!
123

```

## Booleans (flags)

If `true` or `false` is passed for an option having a `bool` argument, it is parsed and bound as expected. But an option whose argument type is `bool` doesn't require an argument to be specified. The presence of the option token on the command line, with no argument following it, results in a value of `true`. You can see various examples here:

``` cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region Bool --session Bool
var command = new RootCommand
            {
                new Option<bool>("--a-bool")
            };

command.Handler = CommandHandler.Create(
    (bool aBool) => Console.WriteLine(aBool));

await command.InvokeAsync("");
await command.InvokeAsync("--a-bool");
await command.InvokeAsync("--a-bool false");
await command.InvokeAsync("--a-bool true");
```

``` console --session Bool
False
True
False
True

```

## Enums

You can bind `enum` types as well. The values are bound by name, and the binding is case insensitive:

``` cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region Enum --session Enum
var command = new RootCommand
            {
                new Option<System.IO.FileAccess>("--an-enum")
            };

command.Handler = CommandHandler.Create(
    (FileAccess anEnum) => Console.WriteLine(anEnum));

await command.InvokeAsync("--an-enum Read");
await command.InvokeAsync("--an-enum READ");
```

``` console --session Enum
Read
Read

```

## Arrays, lists, and other enumerable types

Arguments having various enumerable types can be bound. A number of common types implementing `IEnumerable` are supported. In the next example, try changing the type of the `--items` `Option`'s `Argument` property to `Argument<IEnumerable<string>>` or `Argument<List<string>>`.

``` cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region Enumerables --session Enumerables
var command = new RootCommand
            {
                new Option<string[]>("--items")
            };

command.Handler = CommandHandler.Create(
    (IEnumerable<string> items) =>
    {
        Console.WriteLine(items.GetType());

        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    });

await command.InvokeAsync("--items one two three");
```

``` console --session Enumerables
System.String[]
one
two
three

```

## File system types

Since command line applications very often have to work with the file system, `FileInfo` and `DirectoryInfo` are clearly important for binding to support. Run the following code, then try changing the generic type argument to `DirectoryInfo` and running it again.

``` cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region FileSystemTypes --session FileSystemTypes
var command = new RootCommand
            {
                new Option<FileInfo>("-f").ExistingOnly()
            };

command.Handler = CommandHandler.Create(
    (FileSystemInfo f) =>
    {
        Console.WriteLine($"{f.GetType()}: {f}");
    });

await command.InvokeAsync("-f /path/to/something");
```

``` console --session FileSystemTypes
Usage:
  Binding [options]

Options:
  -f <f>
  --version         Show version information
  -?, -h, --help    Show help and usage information


```

## Anything with a string constructor

But `FileInfo` and `DirectoryInfo` are not special cases. Any type having a constructor that takes a single string parameter can be bound in this way. Go back to the previous example and try using a `Uri` instead.

## More complex types

Binding also supports creating instances of more complex types. If you have a large number of options, this can be cleaner than adding more parameters to your handler. `System.CommandLine` has the default convention of binding `Option` arguments to either properties or constructor parameters by name. The name matching uses the same strategies that are used when matching parameters on a handler method.

In the next sample, the handler accepts an instance of `ComplexType`. Try removing its setters and uncommenting the constructor. Try adding properties, or changing the types or names of its properties.

``` cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region ComplexTypes --session ComplexTypes
public static async Task<int> ComplexTypes()
{
    var command = new Command("the-command")
            {
                new Option<int>("--an-int"),
                new Option<string>("--a-string")
            };

    command.Handler = CommandHandler.Create(
        (ComplexType complexType) =>
        {
            Console.WriteLine(Format(complexType));
        });

    await command.InvokeAsync("--an-int 123 --a-string 456");

    return 0;
}

public class ComplexType
{
    // public ComplexType(int anInt, string aString)
    // {
    //     AnInt = anInt;
    //     AString = aString;
    // }
    public int AnInt { get; set; }
    public string AString { get; set; }
}
```

``` console --session ComplexTypes
AnInt: 123 (System.Int32)
AString: 456 (System.String)


```

## System.CommandLine types

Not everything you might want passed to your handler will necessarily come from parsed command line input. There are a number of types provided by `System.CommandLine` that you can bind to. The following example demonstratres injection of `ParseResult` and `IConsole`. Other types can be passed this way as well.

``` cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region DependencyInjection --session DependencyInjection
var command = new RootCommand
            {
                new Option<string>("--a-string"),
                new Option<int>("--an-int"),
                new Option<System.IO.FileAttributes>("--an-enum"),
            };

command.Handler = CommandHandler.Create(
    (ParseResult parseResult, IConsole console) =>
    {
        console.Out.WriteLine($"{parseResult}");
    });

await command.InvokeAsync("--an-int 123 --a-string \"Hello world!\" --an-enum compressed");
```

``` console --session DependencyInjection
ParseResult: [ Binding [ --an-int <123> ] [ --a-string <Hello world!> ] [ --an-enum <Compressed> ] ]

```
