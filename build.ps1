<#
.SYNOPSIS
    Builds this repo. 
.PARAMETER InstallDir
    Default: ./cli-tools
    Determines where to install the tools for this build session. 
.PARAMETER CliVersion
    Default: 1.0.0-*
    Specified which version of the CLI tools to use to build the repo. Defaults to Preview 2.
.PARAMETER Configuration
    Default: Debug
    Which configuration ("Debug", "Release") to use for building the code. It is used for all the CLI commands. 
.PARAMETER Pack
    Default: false
    Whether to run "dotnet pack" and pack NuGet packages of the commands.
.PARAMETER SkipInstall
    Default: false
    Specifies whether to skip the local installation of the CLI tools. If set to true, te script will try to find the CLI tools on the system path and will check the version (either default or passed in via CliVersion) and if they don't match will error out. 
.PARAMETER SkipRestore
    Default: false
    Specifies whether to skip the restore operation. Note that if this operation is skipped the build may fail if there are no lock files for the projects.  
.PARAMETER SkipTests
    Default: false
    Whether to skip running tests. 
.PARAMETER Verbosity
    Default: normal
    MSBuild verbosity to pass to the build process.
#>
[cmdletbinding()]
param(
    [string]$InstallDir="./.dotnet",
    [string]$CliVersion="1.0.0-rc4-004771",
    [string]$Configuration="Debug",
    [bool]$Pack=$TRUE,
    [bool]$SkipInstall=$FALSE,
    [bool]$SkipTests=$FALSE,
    [bool]$SkipRestore=$FALSE,
    [string]$Verbosity="Normal"
)
$ErrorActionPreference="Stop"
$ProgressPreference="SilentlyContinue"
$LocalDotnet=""
function Say($message) {
    Write-Host "OUTPUT: $message"
}
function Err($message) {
    Write-Host "ERROR: $message" -ForegroundColor "Red"
}
function Die($message) {
    Err($message)
    exit 1
}
if (-Not (Test-Path $InstallDir))
{
    New-Item -Type "directory" -Path $InstallDir 
}

if ($SkipInstall -eq $TRUE) {
    $version = dotnet --version
    if ($version -ne $CliVersion) {
        Die("$CliVersion of the dotnet CLI needs to be installed; remove -SkipInstall switch to download the correct version.")
    }
    $LocalDotnet = "dotnet"
} else {
    Say("Downloading CLI installer...")
    Invoke-WebRequest -Uri "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -OutFile "./$InstallDir/dotnet-install.ps1"
    Say("Installing the CLI requested version ($CliVersion)")
    & ./$InstallDir/dotnet-install.ps1 -Version $CliVersion -InstallDir $InstallDir
    $LocalDotnet = "./$InstallDir/dotnet"
}

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

& $LocalDotnet restore CommandLine-netcore.sln
& $LocalDotnet test CommandLine.Tests/CommandLine.Tests-netcore.csproj -l:trx
& $LocalDotnet pack CommandLine/CommandLine-netcore.csproj