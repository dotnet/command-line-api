# Syntax Concepts and Parser

## Tokens

Each "word" on a command line is a token. The rules of a specific command line application determine whether these tokens are parsed as commands, options, arguments, etc.

## Commands

A command is a token that corresponds to an action that the app will perform. The simplest command line applications have only a root command. The app essentially only does one thing. The way in which it does that one thing might vary, for example based on options and arguments that are passed, but these typically just alter the way in which the app does that one thing.

In `System.CommandLine`, commands are represented by the `Command` class

## Root Command

A root command is a command that represents the app executable itself.

In `System.CommandLine`, it is represented by the `RootCommand` class.

## Subcommands

Some command line apps support subcommands, generally indicating that they can do more than one thing. `dotnet build` and `dotnet restore` are examples of different subcommands.

When an app has subcommands, then the commands above them in the hierarchy typically do nothing by themselves. `dotnet add` is not a complete command because it has the subcommands `dotnet add package` and `dotnet add reference`.

In `System.CommandLine`, subcommands also use the `Command` class, which can be added to a parent command, whether the parent is a `RootCommand` or `Command`.

## Options

An option is a named parameter that can be passed to a command. Options usually follow one of a couple of syntax patterns. In POSIX command lines, you'll often see options represented like this:

```console
> myapp --int-option 123
        ^----------^
```

In Windows command lines, you'll often see options represented like this:

```console
> myapp /int-option 123
        ^---------^
```

While `System.CommandLine` can be configured to use these and other prefixes, the default is the POSIX style.

### Delimiters

In addition to a space delimiting an option from its argument, `=` and `:` are also allowed. The following command lines are equivalent:

```console
> myapp --int-option 123
> myapp --int-option=123
> myapp --int-option:123
```

### Aliases

In both POSIX and Windows command lines, it's common for some options to have aliases, which are usually short forms that are easier to type. In the following help example, `-v` and `--verbose` are aliases for the same option:

```console
-v, --verbose    Show verbose output 
```

In `System.CommandLine`, both the `Command` and `Option` classes support adding [aliases](How-To.md#Add-an-alias-to-an-option-or-command).

## Arguments and arity

An argument is a value passed to an option or command.

```console
> myapp --int-option 123
                     ^-^
                     option argument
       
> myapp --int-option 123 "hello there"
                         ^-----------^
                         command argument
```

Arguments can have default values, expected types, and rules about how many values should be provided ("arity"). The arity of an option or command's argument refers to the number of values that can be passed if that option or command is specified.

Arity is expressed with a minimum value and a maximum value. These are the most common variants:

| Min  | Max  | Examples                |                                |              
|------|------|-------------------------|--------------------------------|
| 0    | 1    | Valid:                  | --flag                         |
|      |      |                         | --flag true                    |
|      |      |                         | --flag false                   |
|      |      | Invalid:                | --flag false false             |
| 1    | 1    | Valid:                  | --file a.json                  |
|      |      | Invalid:                | --file                         |
|      |      |                         | --file a.json b.json           |
| 0    | _n_  | Valid:                  | --file                         |
|      |      |                         | --file a.json                  |
|      |      |                         | --file a.json b.json           |
| 1    | _n_  | Valid:                  | --file a.json                  |
|      |      |                         | --file a.json b.json           |
|      |      | Invalid:                | --file                         |

### Bundling

POSIX recommends that single-character options be allowed to be specified together after a single `-` prefix. The following command lines are equivalent:

```console
> myapp -a -b -c
> myapp -abc
```

If an argument is provided after an option bundle, it applies to the last option in the bundle. The following command lines are equivalent:

```console
> myapp -a -b -c arg
> myapp -abc arg
```

## Directives

`System.CommandLine` introduces a syntactic concept called a *directive*. Here's an example:

```console
> dotnet-interactive [parse] jupyter --verbose /connect.json
                     ^-----^
```

The `[parse]` directive outputs a diagram of the parse result rather than invoking the command line tool:

```console
[ dotnet-interactive [ jupyter [ --verbose <True> ] [ connection-file <connect.json> ] *[ --default-kernel <csharp> ] ] ]
```

Another example of a use for directives is setting the [output rendering mode](Features-overview.md#rendering-directives).

The general goal of directives is to provide cross-cutting functionality that can be made consistent across command line apps. Because directives are syntatically distinct from the app's own parameters, an input such as `[parse]` can be made consistent across apps. An unrecognized directive will be ignored rather than causing a parsing error.

A directive must conform to the following syntax rules:

* It is a token on the command line coming after your app's name but before any subcommands or options, and
* It is enclosed in square brackets.
* It does not contain spaces.
* It can include an argument, separated from the directive name by a colon.

