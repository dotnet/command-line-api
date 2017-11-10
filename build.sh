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

# $args array may have empty elements in it.
# The easiest way to remove them is to cast to string and back to array.
# This will actually break quoted arguments, arguments like 
# -test "hello world" will be broken into three arguments instead of two, as it should.
args=( "$@" )
temp="${args[@]}"
args=($temp)

export XDG_DATA_HOME="$REPOROOT/.nuget/packages"
export NUGET_PACKAGES="$REPOROOT/.nuget/packages"
export NUGET_HTTP_CACHE_PATH="$REPOROOT/.nuget/packages"
export DOTNET_INSTALL_DIR="$REPOROOT/.dotnet"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if [ ! -d "$DOTNET_INSTALL_DIR" ]; then
  mkdir $DOTNET_INSTALL_DIR
fi

curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir $DOTNET_INSTALL_DIR --version 1.0.0-rc4-004911
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --install-dir $DOTNET_INSTALL_DIR --version 2.0.2

PATH="$DOTNET_INSTALL_DIR:$PATH"

dotnet msbuild build.proj /t:MakeVersionProps
dotnet msbuild build.proj /v:diag /fl /flp:v=diag "${args[@]}"
