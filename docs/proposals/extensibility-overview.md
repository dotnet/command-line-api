# System.CommandLine Extensibility

## Overview

`System.CommandLine` is still in preview, yet it had 1.3 million downloads over the past 6 weeks, and is a key dependency of the .NET CLI. There is no other stable first-party story for handling command-line options. This is a problem for authors of command-line tools in the .NET team and in the community, as handling command-line input in a way that is correct and consistent with other tools in the ecosystem is nontrivial.

However, the main reason `System.CommandLine` is in preview is that it has not passed API review and has some open questions and opinionated decisions that have not been fully validated with customers. Due to the large amount of functionality, this is currently difficult to resolve. Some functionality is certain to change, but this is commingled with the parser, which cannot change behavior without breaking scripts that use CLI tools.

This document proposes a progressive approach to landing `System.CommandLine` by breaking it up into layered pieces that can be individually finalized. These pieces are intended to be composable, with tightly scoped default implementations. Developers with custom needs and developers of other command-line libraries (such as `Spectre.Console`) will be able to build on top of these pieces and/or provide richer drop-in replacements.

## Layering

### Parser

Implementing command-line parsing in a complete and consistent way is difficult due to many details such as POSIX behavior, aliases, option bundling, escaping, arity, etc. `System.CommandLine` has a robust and well-validated CLI parser, but it is somewhat coupled with `System.CommandLine`’s implementation of higher-level concerns such as validation, help, and invocation.

The parser has an API for constructing a “grammar”, a tree of nodes describing the CLI’s commands, options and arguments. The parser uses this grammar to parse the args array, producing a parse result.

The parser layer is a low-level API that is not intended to be replaceable and is minimally extensible. It’s a foundational component for higher-level APIs, third-party command-line libraries and developers with custom needs, providing them with a command-line parsing implementation that is complete and correct. This will help promote consistent command-line parsing behavior across the .NET ecosystem.

#### Parse Result

The parser produces a ParseResult that is a structured representation of the argument array analogous to an AST. It contains `CommandResult`, `OptionResult<T>` and `ArgumentResult<T>` objects that represent the commands, arguments and options that were parsed from the args array and preserve the order from the args array. Commands, options and arguments that were defined in the grammar but were not found in the args array will not be present in the `ParseResult`. The result is purely a representation of what was parsed, and determining the value of a specific option or argument (including handling of default values) is the responsibility of higher layers.

These nodes have `ArgsOffset` and `ArgsLength` properties that indicate the portion of the args array from which they were parsed. This allows advanced scenarios such as printing errors with a position marker.

If an option has an optional value, such as bool options treating `--enabled` as equivalent to `--enabled=true`, then the `Option<T>` will have a property indicating whether this optional value was provided or not.

Determining the value of a specific option or argument may involve inspecting other options, or options on parent commands, or using a default value. This will generally be done via the subsystem extensibility layer.

The result also contains a collection of any errors that were found during parsing. These errors use a Roslyn-like descriptor model, where the prototypical error that defines the format string and error code is separate from the error instance, which contains a reference to the descriptor along with position data and format data.

#### Type Conversion

The parser will support C# primitive types in its `Option<T>` and `Argument<T>`. We may also choose to add support for commonly used types such as `DateTime`, `FileInfo`, and `DirectoryInfo`.

Developers will be able to use other types if they provide a converter. However, this type conversion will not use the current CustomParser extensibility model, as it exposes a low-level Symbol/Token API that we would like to keep internal for now.  Instead, it will have a simple, reflection-free type converter that allows will allow `Option<T>` and `Argument<T>` to be used with non-primitive types, without exposing a complex symbol/token API. This model would allow converting any primitive type or array of primitive types:

```csharp
delegate TOutput TypeConverter<TInput,TOutput>(TInput input);

Option<Point> locationOpt = new Option<int[],Point>(“--location”) {
  TypeConverter = (int[] s) => new Point(s[0],s[1]))
}
```

A standard `ParserErrorException` exception type would be used for returning user-visible error messages, and the parser would catch these and add them to its error collection. For convenience, the parser would also collect all other exceptions from type conversion and add a generic “Invalid format” error.

Type converters are only expected to perform parsing and construction. They are not expected to provide any other kind of validation, as that should be handled in the validation subsystem, which is also responsible for printing any `ParserErrors` in the `ParseResult`.

#### Reusable Type Converters

Perhaps we could also allow registering these type converters on the RootCommand so that they do not need to be provided for every option/argument that uses the same type.

```csharp

var rootCommand = new RootCommand()
  .WithTypeConverter<int[],Point>(s => new Point(s[0],s[1]));
```

This would also allow libraries to provide helper methods to register converters on the RootCommand:

```csharp
static RootCommand WithPointConverter(this RootCommand rootCommand)
  => root.WithTypeConverter<int[],Point>(s => new Point(s[0],s[1]));
```

Alternatively, developers using might choose to subclass the option or argument type:

```csharp
class PointOption : Option<int[],Point> {
  public PointOption(string name) : base(name) {
    TypeConverter = s => new Point(s[0],s[1]));  
  }
}
```

### Subsystems

A subsystem operates on the parse result and performs some action based on the parser grammar and parse result. The core subsystems are help, validation, invocation, and completion. Developers using the low-level parser API may use any or all the subsystems directly, and higher-level command-line APIs may use subsystems internally.

The subsystems envisaged are:

* Error handling: print formatted errors.
* Help: set help information, handle the `--help` option, and print formatted help.
* Completion: handle the completion option.
* Validation: set constraints, and check values respect constraints.
* Invocation: set handler delegates, and dispatch to the correct one.
* Defaults: set default values, and determine values for all arguments and options that take these defaults into account.

Subsystems are intended to allow drop-in replacements that go beyond the functionality of the default implementations. For example, an alternate help subsystem may format the help output differently. A developer may obtain an alternate subsystem from a NuGet package or implement their own. Higher-level command-line APIs are expected to use relevant subsystems internally and allow developers to optionally override them with alternate subsystems.

Subsystems may require or be influenced by additional information associated with parser nodes. For example:

* The invocation subsystem requires handler delegates to be attached to command grammar nodes so they can be invoked when that command is present in the parse result.
* The help subsystem’s output can be enriched by adding help descriptions to the command, option and argument grammar nodes.
* An alternate help subsystem may support additional information such as examples or hyperlinks.
* The existing System.CommandLine.NamingConventionBinder could become an alternate invocation layer, allowing strongly typed command handler delegates with parameters corresponding to the command’s options and arguments, and binding them automatically when the command is invoked.

These subsystem annotations do not influence parsing so do not need to be coupled with the parser layer. High-performance scenarios may wish to lazily provide some annotations, such as only loading help descriptions when help is invoked, and alternate subsystems may define and use additional annotations. For this reason, an extensible model for subsystem annotations will be provided at the subsystem layer and is detailed later in this document.

Although alternate subsystems may have additional annotations, they are expected to use the annotations of the default subsystems where possible, so that when alternate subsystems are dropped into an existing application, they use any relevant information that the developer has already provided.

### Binding/Model

A binding/model layer wraps the parser and subsystem layers in a high-level, user-friendly API. In the long term, most developers of CLI apps should be using a model/binding layer. A binding/model layer is by nature opinionated, and there are many different idioms it could use. However, they can all build on top of the parser and subsystems layers, and allow drop-in replacement of subsystems.

An example of a model/binding layer is `System.CommandLine`’s `DragonFruit` API, which allows developers to define their CLI by writing a Main method with strongly typed parameters corresponding to the CLI’s options and arguments. Using a Roslyn generator, DragonFruit internally constructs a parser grammar using the method’s signature, converting default parameter values into default option values, and converting doc comments into help descriptions. On invocation it binds the command-line options and arguments to the method’s arguments and invokes the method.

## Stabilization Status

The parser layer’s API is almost shovel-ready for inclusion in the BCL in .NET 9, and we should be able to re-use much of the implementation and tests from System.CommandLine.

The next target will be to stabilize the subsystems. The subsystems may not become part of the BCL but will be a stable package. When the .NET CLI migrates to this package, this will eliminate its preview dependency, as it does not use a binding/model layer.
The default subsystems will be a straightforward transformation of the existing functionality used by the .NET CLI. However, the subsystem API and annotation APIs will require some discussion.

The model/binding layer requires more experimentation with different forms of binding/model layer to validate with developers before committing to using one for the Console App templates in Visual Studio and the .NET SDK. We may end up with multiple, and there will no doubt be third party ones.

## Subsystem API Pattern

The subsystems follow a common pattern for initialization, annotations, and invocation.

### Initialization

All subsystems have an initialization call to create the subsystem and apply any settings. This initialization call may require a RootCommand instance so it can add options, such as the --help option required by the help subsystem.

One question here is whether subsystems should be locals or should be attached to the `RootCommand` or some other collection such as a new `CliPipeline` class. Ideally the subsystems would not be stored on the `RootCommand` as that would require either putting the concepts of subsystems in the parser layer (e.g. an abstract `CliSubsystem` class and a `Dictionary<Type,Subsystem>` on `RootCommand`) or having a completely generic `PropertyBag`-like object storage mechanism on `RootCommand`, which is not generally considered a good pattern for the BCL. As developers will need to create an instance of a subsystem to opt into using that subsystem, it seems reasonable to store them in locals. The value of storing the subsystem instances in a standard place would be if that made it easier for extension methods to locate the subsystem instance.

The developer must be able to provide an instance of an alternate subsystem, or an instance of the subsystem configured with custom options. The subsystem may also have an optional parameter for an annotation provider, which allows performance-sensitive developers to perform lazy lookup of annotations instead of setting them upfront.

For example, a local-based initialization call might simply be a constructor call:

```csharp
var help = new MyAlternateHelpSubsystem(helpAnnotationProvider);
```

### Annotations

#### Annotation Storage

A subsystem must also provide methods to annotate `CliSymbol` grammar nodes (`CliCommand`, `CliOption` and `CLiArgument`) with arbitrary string-keyed data. An open question is how this data should be attached.

It would be desirable to allow setting symbol-specific annotations directly on the symbol, e.g.

```csharp
command.SetDescription("This is a description");
```

However, this would require the parser layer to be aware of the concept of subsystem annotations and to expose a `PropertyBag`-like model for storage of arbitrary data, which is very unlikely to pass BCL API review. Alternatively the subsystem layer could add subclasses for all the `CliSymbol` derived classes to store this data, but this creates a bifurcation in the usage of the parser API. The last option would be to use a hidden static `ConditionalWeakTable` to associate annotation data with symbol instances, but magically storing instance data in a hidden static field is not a good pattern, and has problematic implications around performance and threading.

Instead, we could make each subsystem responsible for storing its own annotation data. For example, the base `CliSubsystem` could expose the following annotation API:

```csharp
void SetAnnotation<T>(CliSymbol symbol, string id, T value);
T GetAnnotation<T>(CliSymbol symbol, string id);
```

These would internally store the annotation values on a dictionary keyed on the symbol and the annotation ID.

#### Annotation Accessors

Developers would not expected to use these base annotation accessors directly unless they are writing an alternate subsystem that has its own additional annotations. The default subsystem and alternate subsystems should provider wrapper methods for specific annotations.

For example, for help descriptions, the `HelpSubsystem` could have the following accessors:

```csharp
void SetDescription(CliSymbol symbol, string description)
    => SetAnnotation(symbol, HelpAnnotations.Description, description);
string GetDescription(CliSymbol symbol)
    => GetAnnotation<string>(symbol, HelpAnnotations.Description);
```

There would also be static classes defining the IDs of well-known annotations for use by subsystems and annotation providers, such as `HelpAnnotations.Description`.

#### Fluent Annotations

Unfortunately it is not easy to add fluent helpers such as `command.WithHelpDescription(“Some description”)` as such an extension method would not be able to locate the annotation storage unless the annotations were stored on the symbol or accessible via a hidden static `ConditionalWeakTable`, which are problematic for the reasons described earlier.

Even storing the subsystem on the `RootCommand` would not help with this, as in the following example, the `SetHelpDescription` extension methods would not have access to the `RootCommand` instance as the `Command`’s parent is not set until after the `WithHelpDescription` extension method is called:

```csharp
rootCommand.Add(
    new Command(“--hello”)
        .WithHelpDescription(“Hello”));
```

However, a different approach to annotation wrappers would enable a pattern for fluently setting annotations on grammar nodes when constructing the grammar.

The following `AnnotationAccessor<T>` wrapper struct encapsulates a reference to the subsystem and the annotation ID:

```csharp
record struct AnnotationAccessor<T> (Subsystem Subsystem, string Id) {
  public void Set(CliSymbol symbol, T value) => subsystem.SetAnnotation(symbol, Id, value);
  public T Get(CliSymbol symbol) => subsystem.GetAnnotation<T>(symbol, Id);
}
```

Subsystems would be expected to provide properties that expose instances of this wrapper for individual annotations:

```csharp
AnnotationAccessor<string> Description => new (this, HelpAnnotations.Description);
```

This would allow setting an annotation value for a node via these annotation wrappers with the following pattern, replacing the earlier `SetDescription` wrapper method pattern:

```csharp
help.Description.Set(thingCommand, “This is a thing”);
```

Using this pattern instead of the earlier `Set`/`GetDescription` style wrappers would allow the implementation of the following extension method on the grammar nodes:

```csharp
static Command With<T>(this CliSymbol symbol, AnnotationAccessor<T> accessor, T value)
    => accessor.SetValue(symbol, value);
```

This extension method would allow fluently setting the help description in a relatively discoverable and easily readable way:

```csharp
var rootCommand = new RootCommand();
var help = new HelpSubsystem();
rootCommand.Add (
  new Command (“greet”)
    .With(help.Description, “Greet the user”)
);
```

#### Annotation Providers

The annotation provider model allows performance-sensitive developers to opt into lazily fetching annotations when needed. Developers may provide an instance of this provider to a subsystem when initializing the subsystem.

```csharp
interface AnnotationProvider {
  Get(CliSymbol symbol, string id, object value);
}
```

An implementation of one of these methods might look as follows:

```csharp
GetCommand command, string id, object value)
  => (command.Name, id) switch {
    (“greet”, HelpAnnotation.Description) => “Greet the user”,
    _ => null
};
```

It would even be possible to implement a source generator for optimizing CLI apps by converting fluent annotations into lazy annotations. It would collect values passed to the `With<T>(Annotation,T)` extension method, generate annotation provider implementations that provide those value lazily, and elide the `With` method calls with an interceptor.

### Subsystem Invocation

Subsystems provide a method that must be called after parsing to invoke the subsystem. Invocation uses the `ParseResult`, subsystem annotations, and any settings provided when initializing the subsystem.

When invoked subsystems should print any warnings and errors using the error handler subsystem. If not provided an error handler subsystem, they should use the default one, which prints to stderr. An alternate error handler implementation could customize how errors are rendered, or it could collect the errors so that they could be inspected or printed a later point.

If subsystem invocation determines that the app should be terminated, it should return an `ExitDescriptor`, otherwise `null`. This `ExitDescriptor` encapsulates an exit code and a description of the code’s meaning that is intended to be printed only when showing information about all available exit codes and their meanings.

For example, the `HelpSubsystem`’s `ShowIfNeeded` invocation method checks whether the parseResult contains the help option, and if so, it prints help based on the grammar and annotations, and returns `ExitDescriptor.Success` to indicate that help was invoked and the program should exit.

### Subsystem Pipeline

Here is an example of a full result handling pipeline that uses all the subsystems:

```csharp
// if there are any parsing errors, print them, and determines
// which error to use for the exit code and return it
if (errorHandler.TryPrintErrors(parseResult) is CliExit exit) {
  // ExitDescriptor has implicit cast to exit code int
  return error;
}

// if result contains help option, show help and return success exit.
// may use values from the validation and default value
// subsystem to enrich output.
if (help.ShowIfNeeded(parseResult, validation, defaults, errorHandler) is CliExit exit) {
  return exit;
}

// if result contains completion directive, print completion
// and return success exit. may return an error exit if there is some
// internal error.
if (completion.Handle(parseResult, errorHandler) is CliExit exit) {
    return exit;
}

// validate all values in the parse result and print validation
// errors to the errorHandler. if any errors, return appropriate exit.
if (validation.Validate(parseResult, errorHandler) is CliExit exit) {
    return exit 
}
// create a collection that can return values for all options
// and arguments, even if they were not present in the ParseResult.
// if any default value delegate throws exceptions,
// print them using the errorHandler, and returns error exit.
if (CliValues.Create(parseResult, defaults, errorHandler, out CliValues values) is CliExit exit) {
    return exit;
}

// determine which handler delegate to use and invoke it.
// depending how the delegate was registered, may pass the values
// collection to the invocation delegate directly, or bind
// the delegate’s arguments to values from this collection.
// returns the exit descriptor returned from the invoked delegate,
// or null if it did not find a delegate to invoke.
if (invocation.Dispatch(parseResult, values, errorHandler) is CliExit exit) {
    return exit;
}

// creates a customized ExitDescriptor that also prints
// a short form of the help
CliExit noCommandExit = help.CreateNoCommandExit ();
errorHandler.WriteError(noCommandExit);
return noCommandExit;
```

There would be several extension methods that encapsulate the standard subsystem invocation pipeline shown above. The most important is `Invoke`:

```csharp
ExitDescriptor Invoke(
    this parseResult result,
    InvocationSubsystem invocation,
    HelpSubsystem? helpSubsystem = null,
    DefaultValuesSubsystem? defaultValues = null,
    ValidationSubsystem? validationSubsystem = null,
    CompletionSubsystem? completion = null,
    ErrorHandler? errorHandler = null
)
```

Note that the `InvocationSubsystem` subsystem cannot be null for the `Invoke` helper. Note also that the arguments are in the order of most to least likely to be provided , making it more likely arguments can be omitted without passing nulls or using named arguments.

The `GetValues` variant of this helper omits the invocation subsystem, and returns the CliValues and the command, for users who want to perform invocation manually:

```csharp
var (exitDescriptor, command, values) = parseResult.GetValues(
  help, defaultValues, validation, completion, errorHandler
);
```

Note that any of the subsystems passed to these helpers may be null. If the error handle, help, completion, or default value subsystems are null, an instance of the default implementation will be used. The validation and invocation subsystem invocations will be skipped if they are not provided, as they do nothing without annotations so the default instance would be redundant.

Although most users would use these helpers, some very advanced cases may wish to use any or all of the subsystems in a custom pipeline. The main value of this subsystem model is that apps can use alternate implementations for any or all of the subsystems, either written specifically for the app or obtained from NuGet.

## End-to-End Example

Here is an end-to-end example of an entire CLI application that initializes subsystems, constructs a simple command, attaches annotations, and runs the subsystem pipeline:

```csharp
var rootCommand = new RootCommand();

var help = new HelpSubsystem(rootCommand);
var invocation = new InvocationSubsystem(rootCommand);
var defaults = new DefaultValuesSubsystem(rootCommand);

rootCommand.Add (
  new Command (“greet”,
        new Argument<string>(“name”),
        .With (help.Description, “The name of the person to greet”),
        .With (defaults.Provider, () => Environment.UserName)
    )
    .With(help.Description, “Greet the user”)
    .With(invocation.Handler,
      name => Console.WriteLine($“Hello {name}!”))
);

var parseResult = rootCommand.Parse(args);

return parseResult.Invoke (invocation, help, defaults);
```
