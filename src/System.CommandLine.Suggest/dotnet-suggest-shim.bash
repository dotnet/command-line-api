_dotnet_bash_complete()
{
    local fullpath=`type -p ${COMP_WORDS[0]}`
    local completions=`dotnet-suggest get --executable "${fullpath}" --position ${COMP_POINT} -- ${COMP_LINE}`

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

complete -F _dotnet_bash_complete `dotnet-suggest list`