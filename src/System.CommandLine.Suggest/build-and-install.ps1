function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath
}

dotnet pack /p:Version=2.0.0
dotnet tool uninstall -g dotnet-suggest
dotnet tool install -g --add-source "$(Get-ScriptDirectory)/bin/debug" --version 2.0.0 dotnet-suggest