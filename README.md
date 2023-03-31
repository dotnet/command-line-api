

[![Build Status](https://dev.azure.com/dnceng-public/public/_apis/build/status/dotnet/command-line-api/command-line-api?branchName=main)](https://dev.azure.com/dnceng-public/public/_build?definitionId=175&branchName=main) [![Join the chat at https://gitter.im/dotnet/command-line-api](https://badges.gitter.im/dotnet/command-line-api.svg)](https://gitter.im/dotnet/command-line-api?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This repository contains the code for the System.CommandLine libraries and the `dotnet-suggest` global tool.

## Packages

Package                          | Version                                                                                                                                     | Description
---------------------------------| ------------------------------------------------------------------------------------------------------------------------------------------- | -----------------------------
`System.CommandLine`             | [![Nuget](https://img.shields.io/nuget/v/System.CommandLine.svg)](https://nuget.org/packages/System.CommandLine)                            | Command line parser, model binding, invocation, shell completions
`System.CommandLine.DragonFruit` | [![Nuget](https://img.shields.io/nuget/v/System.CommandLine.DragonFruit.svg)](https://nuget.org/packages/System.CommandLine.DragonFruit)    | Build command-line apps by convention with a strongly-typed `Main` method
`System.CommandLine.Hosting`     | [![Nuget](https://img.shields.io/nuget/v/System.CommandLine.Hosting.svg)](https://nuget.org/packages/System.CommandLine.Hosting)            | support for using System.CommandLine with [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/)
`dotnet-suggest`                 | [![Nuget](https://img.shields.io/nuget/v/dotnet-suggest.svg)](https://nuget.org/packages/dotnet-suggest)                                    | A command-line tool to provide shell completions for apps built using `System.CommandLine`.

### Daily Builds

Daily builds are available if you add this feed to your nuget.config: `https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/index.json`

Versions are listed at: https://dev.azure.com/dnceng/public/_artifacts/feed/dotnet-libraries/NuGet/System.CommandLine/versions

## Documentation

The System.CommandLine documentation can now be found at [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/commandline/).

## Code of Conduct

This project has adopted the code of conduct defined by the [Contributor Covenant](https://www.contributor-covenant.org/) to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](https://www.dotnetfoundation.org/code-of-conduct)

## Contributing

See the [Contributing guide](CONTRIBUTING.md) for developer documentation.

## License

This project is licensed under the [MIT license](LICENSE.md).

## .NET Foundation

.NET is a [.NET Foundation](http://www.dotnetfoundation.org/projects) project.
