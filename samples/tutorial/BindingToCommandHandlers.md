- [Binding](./Binding.md)
- **Binding to a command handler**

# Binding to a command handler

The simplest way to bind command line input is to set the `Command.Handler` property. The `System.CommandLine` model binder will look at the options and arguments for the command and attempt to match them to the parameters of the specified handler method. The default convention is that parameters are matched by name, so in the following example, option `--an-int` matches the parameter named `anInt`. Matching ignores hyphens (and other option prefixes, such as `'/'`) and is case insensitive.

```cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region MultipleArgs 
```

## Booleans (flags)

If `"true"` or `"false"` is passed for an option having a `bool` argument, it is parsed and bound as expected. But an option whose argument type is `bool` doesn't require an argument to be specified. The presence of the option token on the command line, with no argument following it, results in a value of `true`. You can see various examples here:

```cs  --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region Bool 
```

## Enums

You can bind `enum` types as well. The values are bound by name, and the binding is case insensitive:

```cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region Enum 
```

## Arrays, lists, and other enumerable types 

Arguments having various enumerable types can be bound. A number of common types implementing `IEnumerable` are supported. In the next example, try changing the type of the `--items` `Option`'s `Argument` property to `Argument<IEnumerable<string>>` or `Argument<List<string>>`. 

```cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region Enumerables 
```

## File system types

Since command line applications very often have to work with the file system, `FileInfo` and `DirectoryInfo` are clearly important for binding to support. Run the following code, then try changing the generic type argument to `DirectoryInfo` and running it again.

```cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region FileSystemTypes 
```

## Anything with a string constructor

But `FileInfo` and `DirectoryInfo` are not special cases. Any type having a constructor that takes a single string parameter can be bound in this way. Go back to the previous example and try using a `Uri` instead.

## More complex types

Binding also supports creating instances of more complex types. If you have a large number of options, this can be cleaner than adding more parameters to your handler. `System.CommandLine` has the default convention of binding `Option` arguments to either properties or constructor parameters by name. The name matching uses the same strategies that are used when matching parameters on a handler method. 

In the next sample, the handler accepts an instance of `ComplexType`. Try removing its setters and uncommenting the constructor. Try adding properties, or changing the types or names of its properties.

```cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region ComplexTypes 
```

## System.CommandLine types

Not everything you might want passed to your handler will necessarily come from parsed command line input. There are a number of types provided by `System.CommandLine` that you can bind to. The following example demonstratres injection of `ParseResult` and `IConsole`. Other types can be passed this way as well.

```cs --source-file ./src/Binding/HandlerBindingSample.cs --project ./src/Binding/Binding.csproj --region DependencyInjection 
```





