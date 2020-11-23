# dotnet suggest shell complete script start
_dotnet_bash_complete()
{
    local fullpath=`type -p ${COMP_WORDS[0]}`
    local escaped_comp_line=$(echo "$COMP_LINE" | sed s/\"/'\\\"'/g)
    local completions=`dotnet-suggest get --executable "${fullpath}" --position ${COMP_POINT} -- "${escaped_comp_line}"`

    if [ "${#COMP_WORDS[@]}" != "2" ]; then
        return
    fi

    local IFS=$'\n'
    local suggestions=($(compgen -W "$completions"))

    if [ "${#suggestions[@]}" == "1" ]; then
        local number="${suggestions[0]/%\ */}"
        COMPREPLY=("$number")
    else
        for i in "${!suggestions[@]}"; do
            suggestions[$i]="$(printf '%*s' "-$COLUMNS"  "${suggestions[$i]}")"
        done

        COMPREPLY=("${suggestions[@]}")
    fi
}

_dotnet_bash_register_complete()
{
    local IFS=$'\n'
    complete -F _dotnet_bash_complete `dotnet-suggest list`
}
_dotnet_bash_register_complete
export DOTNET_SUGGEST_SCRIPT_VERSION="1.0.1"
# dotnet suggest shell complete script end
