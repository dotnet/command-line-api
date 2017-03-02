export DOTNET_INSTALL_DIR=.dotnet
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
mkdir $DOTNET_INSTALL_DIR
curl -sSL https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.sh | bash /dev/stdin --install-dir .dotnet
PATH="$DOTNET_INSTALL_DIR:$PATH"

dotnet restore CommandLine-netcore.sln -r osx.10.11-x64 
dotnet build CommandLine-netcore.sln -r osx.10.11-x64 
dotnet publish ./dotnet/dotnet-netcore.csproj -r osx.10.11-x64 -f netcoreapp1.0
chmod +x ./dotnet/bin/Debug/netcoreapp1.0/osx.10.11-x64/publish/dotnet