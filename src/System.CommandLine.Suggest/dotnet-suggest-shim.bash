_dotnet_bash_complete()
{
    local fullpath=$(type -p "${COMP_WORDS[0]}")
    local completions=$(dotnet-suggest get --executable "${fullpath}" --position "${COMP_POINT}" -- "${COMP_LINE}")

    # filter suggestions by word to complete.
    local word="${COMP_WORDS[COMP_CWORD]}"
    local IFS=$'\n'
    local suggestions=($(compgen -W "$completions" -- "$word"))

    # format suggestions as shell input.
    for i in "${!suggestions[@]}"; do
        suggestions[i]="$(printf '%q' "${suggestions[$i]}")"
    done

    COMPREPLY=("${suggestions[@]}")
}

_dotnet_bash_register_complete()
{
    if command -v dotnet-suggest &>/dev/null; then
        local IFS=$'\n'
        complete -F _dotnet_bash_complete $(dotnet-suggest list)
    fi
}

_dotnet_bash_register_complete
export DOTNET_SUGGEST_SCRIPT_VERSION="1.0.3"
