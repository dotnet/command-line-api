Contributing
============

Please read [.NET Core Guidelines](https://github.com/dotnet/runtime/blob/master/CONTRIBUTING.md) for more general information about coding styles, source structure, making pull requests, and more.
While this project is in the early phases of development, some of the guidelines in this document -- such as API reviews -- do not yet apply as strongly.
That said, please open a GitHub issue to discuss any API renames or changes before submitting PRs.

## Developer guide

This project can be developed on any platform. To get started, follow instructions for your OS.

### Prerequisites

This project depends on .NET Core 2.0. Before working on the project, check that .NET Core prerequisites have been met.

 - [Prerequisites for .NET Core on Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)
 - [Prerequisites for .NET Core on Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x)
 - [Prerequisites for .NET Core on macOS](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x)

### Visual Studio

This project supports [Visual Studio 2017](https://visualstudio.com) and [Visual Studio for Mac](https://www.visualstudio.com/vs/visual-studio-mac/). Any version, including the free Community Edition, should be sufficient so long as you install Visual Studio support for .NET Core development.

This project also supports using
[Visual Studio Code](https://code.visualstudio.com). Install the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) and install the [.NET Core CLI](https://get.dot.net/core) to get started.

### Command line scripts

This project can be built on the command line using the `build.cmd`/`build.sh` scripts.

Run `.\build.cmd -help` or `./build.sh --help` to see more options.

### Compile

Windows:

    > .\build.cmd

macOS/Linux

    $ ./build.sh

### Running tests

To build **and** run tests:

Windows:

    > .\build.cmd -test

macOS/Linux:

    $ ./build.sh --test

