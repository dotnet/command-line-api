# dotnet-suggest

Command line apps built using `System.CommandLine` have built-in support for tab completion.

![t-rex-suggestions](https://user-images.githubusercontent.com/547415/50387753-ef4c1280-06b8-11e9-90c8-89466d0bb406.gif)

On the machine where you'd like to enable completion, you'll need to do two things.

1. Install the `dotnet-suggest` global tool:

```sh
dotnet tool install -g dotnet-suggest
```

2. Install the completion script.

### bash
```sh
dotnet-suggest script bash >~/.dotnet-suggest-shim.bash
echo '. ~/.dotnet-suggest-shim.bash' >>~/.bashrc
```

### zsh
```sh
dotnet-suggest script zsh >~/.dotnet-suggest-shim.zsh
echo '. ~/.dotnet-suggest-shim.zsh' >>~/.zshrc
```

### PowerShell

Add the contents of [dotnet-suggest-shim.ps1](https://github.com/dotnet/command-line-api/blob/master/src/System.CommandLine.Suggest/dotnet-suggest-shim.ps1) to your PowerShell profile. You can find the expected path to your PowerShell profile by running the following in your console:

```console
> echo $profile
```

(For other shells, please [look for](https://github.com/dotnet/command-line-api/issues?q=is%3Aissue+is%3Aopen+label%3A%22shell+suggestion%22) or open an [issue](https://github.com/dotnet/command-line-api/issues).)
