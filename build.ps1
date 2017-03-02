[cmdletbinding()]
param(
    [Parameter(Position=0, ValueFromRemainingArguments=$true)]
    $ExtraParameters
)
$ErrorActionPreference="Stop"
$ProgressPreference="SilentlyContinue"

$RepoRoot = "$PSScriptRoot"
$DOTNET_INSTALL_DIR="$REPOROOT/.dotnet"

$env:XDG_DATA_HOME="$REPOROOT/.nuget/packages"
$env:NUGET_PACKAGES="$REPOROOT/.nuget/packages"
$env:NUGET_HTTP_CACHE_PATH="$REPOROOT/.nuget/packages"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if (-Not (Test-Path $DOTNET_INSTALL_DIR))
{
    New-Item -Type "directory" -Path $DOTNET_INSTALL_DIR 
}

Invoke-WebRequest -Uri "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -OutFile "$DOTNET_INSTALL_DIR/dotnet-install.ps1"
& $DOTNET_INSTALL_DIR/dotnet-install.ps1 -Version $CliVersion -InstallDir $InstallDir

$env:PATH="$DOTNET_INSTALL_DIR;$env:PATH"

& dotnet msbuild build.proj /v:diag /fl /flp:v=diag $ExtraParameters
