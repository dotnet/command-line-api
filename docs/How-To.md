# How To

## Add an alias to an option or command

Both commands and options support [aliases](Concepts.md#Aliases). You can add an alias to an option like this:

``` csharp
var option = new Option("--verbose");
option.AddAlias("-v");
```

Given this alias, the following command lines will be equivalent:

```console
> myapp -v
> myapp --verbose
```

Command aliases work the same way.

```csharp
var command = new Command("serialize");
command.AddAlias("serialise");
```

The following command lines will be equivalent:

```console
> myapp serialize
> myapp serialise
```

## Add a subcommand (or verb)

Commands can have child commands, often called verbs, and these can nest as many levels as you like. You can add a subcommand like this:

```csharp
var parent = RootCommand("parent");
var child = new Command("child");
parent.Add(child);
var grandchild = new Command("grandchild");
child.Add(grandchild);
```

The innermost subcommand in this example can be invoked like this:

```console
> parent child grandchild
```

Collection initializer syntax is supported, so the following is equivalent:

```csharp
var parent = new RootCommand("parent")
{
    new Command("child")
    {
        new Command("grandchild")
    }
};
```

## Call a method

The simplest case for invoking your code, if you have a program so simple that it has no inputs beyond invocation itself, would look like this:

```csharp
static void Main()
{
    var rootCommand = new RootCommand();

    rootCommand.Handler = CommandHandler.Create(() =>
    {
        /* do something */
    });

    rootCommand.InvokeAsync(args).Wait();
}
```

Of course, if your program is so simple that is has no inputs, you probably didn't need a command line parser and you can `/* do something */` directly in the body of `Main`. Nonetheless, this will give you some additional [features](Features-overview).

## Pass parameters to a method

Usually, your `/* do something */` method has parameters and you would like these to be specified using command line options. 

```csharp
public static void DoSomething(int anInt, string aString)
{
   /* do something */
}
```

This is known as binding. You can learn more about it in the interactive tutorial described in the readme: https://github.com/dotnet/command-line-api#interactive-tutorials

There are currently two models for configuring the `System.CommandLine` parser to bind these parameters.

### Syntax-first

One approach that you can use is to configure the parser directly by adding `Option`s to your `RootCommand`. **Note that the option names should match the names of the parameters of the `DoSomething` method.** 

Parameters are matched using a naming convention that converts camel-cased parameters to kebab-cased options. In this example, the option `--an-int` matches parameter `anInt` on the `DoSomething` method.

```csharp
static void Main()
{
    var rootCommand = new RootCommand();

    rootCommand.Add(new Option<int>("--an-int"));
    rootCommand.Add(new Option<string>("--a-string"));

    rootCommand.Handler = CommandHandler.Create<int, string>(DoSomething);

    rootCommand.InvokeAsync(args).Wait();
}

public static void DoSomething(int anInt, string aString)
{
    /* do something */
}
```

### Method-first

Another approach is to let `System.CommandLine` configure the parser for you based on your method signature using the `Command.ConfigureFromMethod` extension method found in the `System.CommandLine.DragonFruit` library. (The  [DragonFruit](Your-first-app-with-System.CommandLine.DragonFruit.md) app model uses this approach for its strongly-typed `Main` method but it can be used with any method.)


```csharp
static void Main()
{
    var rootCommand = new RootCommand();

    MethodInfo method = typeof(Program).GetMethod(nameof(DoSomething));

    rootCommand.ConfigureFromMethod<int, string>(method);

    rootCommand.InvokeAsync(args).Wait();
}
```

`ConfigureFromMethod` adds options to your command based on the parameters of the specified method. Options are created using a naming convention that converts camel-cased parameters to kebab-cased options. In this example, the parameter `anInt` generates an option with the alias `--an-int`;

## Argument validation and binding

Arguments can have default values, expected types, and configurable arity. `System.CommandLine` will reject arguments that don't match these expectations. 

In this example, a parse error is displayed because the input "not-an-int" could not be converted to an `int`:

```console
> myapp --int-option not-an-int
Cannot parse argument 'not-an-int' as System.Int32.
```

In this example, too many arguments are being passed to `--int-option`:

```console
> myapp --int-option 1 --int-option 2
Option '--int-option' expects a single argument but 2 were provided.
```

This is an example of an arity error. `--int-option` has an arity of exactly one (`ArgumentArity.ExactlyOne`), meaning that if the option is specified, a single argument must also be provided.

Boolean options, sometimes called "flags", have an arity of `ArgumentArity.ZeroOrOne`. This is because all of the following are valid ways to specify a `bool` option:

```console
> myapp --bool-option
The value of intOption is: 42
The value of boolOption is: True
The value of fileOption is: null

> myapp --bool-option true
The value of intOption is: 42
The value of boolOption is: True
The value of fileOption is: null

> myapp --bool-option false
The value of intOption is: 42
The value of boolOption is: False
The value of fileOption is: null
```

`System.CommandLine` also knows how to bind other argument types. For example, enums and file system objects such as `FileInfo` and `DirectoryInfo` can be bound. `FileInfo` and `DirectoryInfo` examples of a more general convention whereby any type that has a constructor taking a single `string` parameter can be bound without having to write any custom code. But you can also write your own binding logic for your custom types. 

## Middleware Pipeline

While each command has a handler which `System.CommandLine` will route to based on input, there is also a mechanism for short circuiting or altering the input before invoking you application logic. In between parsing and invocation, there is a chain of responsibility, which you can customize. A number of features of `System.CommandLine` make use of this. This is how the `--help` and `--version` options short circuit calls to your handler. 

Each call in the pipeline can take action based on the `ParseResult` and return early, or choose to call the next item in the pipeline. The `ParseResult` can even be replaced during this phase. The last call in the chain is the handler for the specified command.

You can add a call to this pipeline by calling `CommandLineBuilder.UseMiddleware.` Here's an example that enables a custom directive:

```csharp
commandLineBuilder.UseMiddleware(async (context, next) => {
    if (context.ParseResult.Directives.Contains("just-say-hi"))
    {
        context.Console.Out.WriteLine("Hi!");
    } 
    else
    {
        await next(context);
    }
});
```
```console
> myapp [just-say-hi] --int-option 1234
Hi!
```

In the code above, the middleware writes out "Hi!" if the directive "just-say-hi" is found in the parse result. When this happens, because the provided `next` delegate is not called, then the command's normal handler is not invoked.
