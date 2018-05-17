#!/bin/bash

build=false
ci=false
configuration="Debug"
help=false
log=false
pack=false
prepareMachine=false
rebuild=false
restore=false
sign=false
solution=""
test=false
verbosity="minimal"
properties=()

while [[ $# -gt 0 ]]; do
  lowerI="$(echo "$1" | awk '{print tolower($0)}')"
  case $lowerI in
    --build)
      build=true
      shift 1
      ;;
    --ci)
      ci=true
      shift 1
      ;;
    --configuration)
      configuration=$2
      shift 2
      ;;
    --help)
      help=true
      shift 1
      ;;
    --log)
      log=true
      shift 1
      ;;
    --pack)
      pack=true
      shift 1
      ;;
    --prepareMachine)
      prepareMachine=true
      shift 1
      ;;
    --rebuild)
      rebuild=true
      shift 1
      ;;
    --restore)
      restore=true
      shift 1
      ;;
    --sign)
      sign=true
      shift 1
      ;;
    --solution)
      solution=$2
      shift 2
      ;;
    --test)
      test=true
      shift 1
      ;;
    --verbosity)
      verbosity=$2
      shift 2
      ;;
    *)
      properties+=("$1")
      shift 1
      ;;
  esac
done

function PrintUsage {
  echo "Common settings:"
  echo "  --configuration <value>  Build configuration Debug, Release"
  echo "  --verbosity <value>      Msbuild verbosity (q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic])"
  echo "  --help                   Print help and exit"
  echo ""
  echo "Actions:"
  echo "  --restore                Restore dependencies"
  echo "  --build                  Build solution"
  echo "  --rebuild                Rebuild solution"
  echo "  --test                   Run all unit tests in the solution"
  echo "  --perf                   Run all performance tests in the solution"
  echo "  --sign                   Sign build outputs"
  echo "  --pack                   Package build outputs into NuGet packages and Willow components"
  echo ""
  echo "Advanced settings:"
  echo "  --dogfood                Setup a dogfood environment using the local build"
  echo "                           For this to have an effect, you will need to source the build script."
  echo "                           If this option is specified, any actions (such as --build or --restore)"
  echo "                           will be ignored."
  echo "  --solution <value>       Path to solution to build"
  echo "  --ci                     Set when running on CI server"
  echo "  --log                    Enable logging (by default on CI)"
  echo "  --prepareMachine         Prepare machine for CI run"
  echo ""
  echo "Command line arguments not listed above are passed through to MSBuild."
}

function CreateDirectory {
  if [ ! -d "$1" ]
  then
    mkdir -p "$1"
  fi
}

function GetVersionsPropsVersion {
  echo "$( awk -F'[<>]' "/<$1>/{print \$3}" "$VersionsProps" )"
}

function InstallDotNetCli {
  DotNetCliVersion="$( GetVersionsPropsVersion DotNetCliVersion )"
  DotNetInstallVerbosity=""

  if [ -z "$DOTNET_INSTALL_DIR" ]
  then
    export DOTNET_INSTALL_DIR="$ArtifactsDir/.dotnet/$DotNetCliVersion"
  fi

  DotNetRoot=$DOTNET_INSTALL_DIR
  DotNetInstallScript="$DotNetRoot/dotnet-install.sh"

  if [ ! -a "$DotNetInstallScript" ]
  then
    CreateDirectory "$DotNetRoot"
    curl "https://dot.net/v1/dotnet-install.sh" -sSL -o "$DotNetInstallScript"
  fi

  if [[ "$(echo "$verbosity" | awk '{print tolower($0)}')" == "diagnostic" ]]
  then
    DotNetInstallVerbosity="--verbose"
  fi

  # Install a stage 0
  SdkInstallDir="$DotNetRoot/sdk/$DotNetCliVersion"

  if [ ! -d "$SdkInstallDir" ]
  then
    bash "$DotNetInstallScript" --version "$DotNetCliVersion" $DotNetInstallVerbosity
    LASTEXITCODE=$?

    if [ $LASTEXITCODE != 0 ]
    then
      echo "Failed to install stage0"
      return $LASTEXITCODE
    fi
  fi

  # Put the stage 0 on the path
  export PATH="$DotNetRoot:$PATH"

  # Disable first run since we want to control all package sources
  export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

  # Don't resolve runtime, shared framework, or SDK from other locations
  export DOTNET_MULTILEVEL_LOOKUP=0
}

function InstallRepoToolset {
  RepoToolsetVersion="$( GetVersionsPropsVersion RoslynToolsRepoToolsetVersion )"
  RepoToolsetDir="$NuGetPackageRoot/roslyntools.repotoolset/$RepoToolsetVersion/tools"
  RepoToolsetBuildProj="$RepoToolsetDir/Build.proj"

  if $ci || $log
  then
    CreateDirectory "$LogDir"
    logCmd="/bl:$LogDir/Build.binlog"
  else
    logCmd=""
  fi

  if [ ! -d "$RepoToolsetBuildProj" ]
  then
    ToolsetProj="$ScriptRoot/Toolset.proj"
    dotnet msbuild "$ToolsetProj" /t:restore /m /nologo /clp:Summary /warnaserror "/v:$verbosity" $logCmd
    LASTEXITCODE=$?

    if [ $LASTEXITCODE != 0 ]
    then
      echo "Failed to build $ToolsetProj"
      return $LASTEXITCODE
    fi
  fi
}

function Build {
  if ! InstallDotNetCli
  then
    return $?
  fi

  if ! InstallRepoToolset
  then
    return $?
  fi

  if $prepareMachine
  then
    CreateDirectory "$NuGetPackageRoot"
    dotnet nuget locals all --clear
    LASTEXITCODE=$?

    if [ $LASTEXITCODE != 0 ]
    then
      echo "Failed to clear NuGet cache"
      return $LASTEXITCODE
    fi
  fi

  if $ci || $log
  then
    CreateDirectory "$LogDir"
    logCmd="/bl:$LogDir/Build.binlog"
  else
    logCmd=""
  fi

  if [ -z "$solution" ]
  then
    solution="$RepoRoot/System.CommandLine.sln"
  fi

  # We don't currently pass down /p:Sign=$sign because SignTool only runs on the desktop framework
  dotnet msbuild $RepoToolsetBuildProj /m /nologo /clp:Summary /warnaserror "/v:$verbosity" $logCmd "/p:Configuration=$configuration" "/p:SolutionPath=$solution" /p:Restore=$restore /p:Build=$build /p:Rebuild=$rebuild /p:Test=$test /p:Pack=$pack /p:CIBuild=$ci "${properties[@]}"
  LASTEXITCODE=$?

  if [ $LASTEXITCODE != 0 ]
  then
    echo "Failed to build $RepoToolsetBuildProj"
    return $LASTEXITCODE
  fi
}

function StopProcesses {
  echo "Killing running build processes..."
  pkill -9 "msbuild"
  pkill -9 "vbcscompiler"
}

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  ScriptRoot="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$ScriptRoot/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
ScriptRoot="$( cd -P "$( dirname "$SOURCE" )" && pwd )"

if $help
then
  PrintUsage
  exit 0
fi

RepoRoot="$ScriptRoot/.."
if [ -z $DOTNET_SDK_ARTIFACTS_DIR ]
then
  ArtifactsDir="$RepoRoot/artifacts"
else
  ArtifactsDir="$DOTNET_SDK_ARTIFACTS_DIR"
fi

ArtifactsConfigurationDir="$ArtifactsDir/$configuration"
LogDir="$ArtifactsConfigurationDir/log"
VersionsProps="$ScriptRoot/Versions.props"

# HOME may not be defined in some scenarios, but it is required by NuGet
if [ -z "$HOME" ]
then
  export HOME="$ArtifactsDir/.home/"
  CreateDirectory "$HOME"
fi

if $ci
then
  TempDir="$ArtifactsConfigurationDir/tmp"
  CreateDirectory "$TempDir"

  export TEMP="$TempDir"
  export TMP="$TempDir"
fi

if [ -z "$NUGET_PACKAGES" ]
then
  export NUGET_PACKAGES="$HOME/.nuget/packages"
fi

NuGetPackageRoot=$NUGET_PACKAGES

Build
LASTEXITCODE=$?

if $ci && $prepareMachine
then
  StopProcesses
fi
