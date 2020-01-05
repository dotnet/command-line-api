

[![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/dotnet/command-line-api/command-line-api?branchName=master)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=337&branchName=master) [![Join the chat at https://gitter.im/dotnet/command-line-api](https://badges.gitter.im/dotnet/command-line-api.svg)](https://gitter.im/dotnet/command-line-api?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This repository contains the code for the System.CommandLine libraries and the `dotnet-suggest` global tool. For more information, see [our wiki](https://github.com/dotnet/command-line-api/wiki).

## Packages

Package                          | Version                                                                                                                                     | Description
---------------------------------| ------------------------------------------------------------------------------------------------------------------------------------------- | -----------------------------
`System.CommandLine`             | [![Nuget](https://img.shields.io/nuget/v/System.CommandLine.svg)](https://nuget.org/packages/System.CommandLine)                            | Command line parser, model binding, invocation, shell completions
`System.CommandLine.DragonFruit` | [![Nuget](https://img.shields.io/nuget/v/System.CommandLine.DragonFruit.svg)](https://nuget.org/packages/System.CommandLine.DragonFruit)    | Build command-line apps by convention with a strongly-typed `Main` method
`System.CommandLine.Rendering`   | [![Nuget](https://img.shields.io/nuget/v/System.CommandLine.Rendering.svg)](https://nuget.org/packages/System.CommandLine.Rendering)        | Structured terminal output rendering and ANSI support
`System.CommandLine.Hosting`     | [![Nuget](https://img.shields.io/nuget/v/System.CommandLine.Hosting.svg)](https://nuget.org/packages/System.CommandLine.Hosting)            | support for using System.CommandLine with [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/)
`dotnet-suggest`                 | [![Nuget](https://img.shields.io/nuget/v/dotnet-suggest.svg)](https://nuget.org/packages/dotnet-suggest)                                    | A command-line tool to provide shell completions for apps built using `System.CommandLine`.

Daily builds are available if you add this feed to your nuget.config: https://dotnetfeed.blob.core.windows.net/dotnet-core/index.json.

## Interactive tutorials

You can try out `System.CommandLine` using an interactive tutorial that showcases its features and APIs, powered by Try .NET.

![binding](https://user-images.githubusercontent.com/547415/58752436-905aa880-8463-11e9-9ab7-c2a8136b0a93.gif)

To use the tutorial, first clone the `command-line-api` repository:

```console
> git clone https://github.com/dotnet/command-line-api
```

Next, install the `dotnet try` global tool:

```console
> dotnet tool install -g dotnet-try
```

Finally, launch the `dotnet try` pointing to the tutorial directory inside the cloned repository:

```console
> dotnet try <PATH_TO_COMMAND_LINE_API_REPO>/samples/tutorial
```

## Contributing

See the [Contributing guide](CONTRIBUTING.md) for developer documentation.

## License

This project is licensed under the [MIT license](LICENSE.TXT).

## .NET Foundation

.NET is a [.NET Foundation](http://www.dotnetfoundation.org/projects) project.
