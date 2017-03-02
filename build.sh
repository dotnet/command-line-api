set -e

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ "$SOURCE" != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done

DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
REPOROOT="$DIR"

# Some things depend on HOME and it may not be set. We should fix those things, but until then, we just patch a value in
if [ -z "$HOME" ]; then
    export HOME="$DIR/.home"

    [ ! -d "$HOME" ] || rm -Rf $HOME
    mkdir -p $HOME
fi

export XDG_DATA_HOME="$REPOROOT/.nuget/packages"
echo XDG_DATA_HOME=$XDG_DATA_HOME
export NUGET_PACKAGES="$REPOROOT/.nuget/packages"
echo NUGET_PACKAGES=$NUGET_PACKAGES
export NUGET_HTTP_CACHE_PATH="$REPOROOT/.nuget/packages"
echo NUGET_HTTP_CACHE_PATH=$NUGET_HTTP_CACHE_PATH
export DOTNET_INSTALL_DIR="$REPOROOT/.dotnet"
echo DOTNET_INSTALL_DIR=$DOTNET_INSTALL_DIR
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if [ ! -d "$DOTNET_INSTALL_DIR" ]; then 
	mkdir $DOTNET_INSTALL_DIR
	curl -sSL https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.sh | bash /dev/stdin --install-dir $DOTNET_INSTALL_DIR
fi

PATH="$DOTNET_INSTALL_DIR:$PATH"

dotnet restore CommandLine-netcore.sln /v:diag
dotnet test CommandLine.Tests/CommandLine.Tests-netcore.csproj -l:trx
#dotnet publish ./dotnet/dotnet-netcore.csproj -r osx.10.11-x64 -f netcoreapp1.0 /v:diag
#chmod +x ./dotnet/bin/Debug/netcoreapp1.0/osx. 10.11-x64/publish/dotnet