# dotnet suggest shell complete script start
_dotnet_zsh_complete()
{
    local fullpath=`which ${words[1]}`
    local position line
    read -nl position
    position=$(($position-1))
    read -l line
    line=$(echo "${line}" | sed s/\"/'\\\"'/g)
    local completions=`dotnet-suggest get --executable "$fullpath" --position ${position} -- "${line}"`
    reply=( "${(ps:\n:)completions}" )
}
compctl -K _dotnet_zsh_complete + -f `dotnet-suggest list`
export DOTNET_SUGGEST_SCRIPT_VERSION="1.0.0"
# dotnet suggest shell complete script end
