Contributing
============

Please read [.NET Guidelines](https://github.com/dotnet/runtime/blob/master/CONTRIBUTING.md) for more general information about coding styles, source structure, making pull requests, and more.

## Developer guide

This project can be developed on any platform. To get started, follow instructions for your OS.

### Prerequisites

This project depends on .NET 7. Before working on the project, check that the [.NET SDK](https://dotnet.microsoft.com/en-us/download) is installed.

### Visual Studio

This project supports [Visual Studio 2022](https://visualstudio.com). Any version, including the free Community Edition, should be sufficient.

This project also supports using
[Visual Studio Code](https://code.visualstudio.com). Install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit).

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

