function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath
}

dotnet pack /p:Version=0.0.0-dev 
dotnet tool uninstall -g dotnet-suggest
dotnet tool install -g --version 0.0.0-dev --add-source "$(Get-ScriptDirectory)/bin/Debug" dotnet-suggest

# enable debug logging
$env:DOTNET_SUGGEST_LOGGING=1
