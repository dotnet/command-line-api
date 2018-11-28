#!/usr/bin/env bash

echo "hi"
get_completion_function_name_from_complete_output()
{
    local output=("$@")
    local i
    for ((i = 0; i < "${#output[@]}"; i++)); do
        if [[ "${output[i]}" == "-F" ]]; then
           echo "${output[((i+1))]}"
           return 0
        fi
    done
    return 1
}

get_completions()
{
    COMP_WORDS=("$@") # completion to test
    #echo "COMP_WORDS: '${COMP_WORDS[@]}'"
    COMP_LINE="$(local IFS=" "; echo "${COMP_WORDS[@]}")"
    #echo "COMP_LINE: '$COMP_LINE'"
    COMP_CWORD=$((${#COMP_WORDS[@]} - 1)) # index into COMP_WORDS
    #echo "COMP_CWORD: '$COMP_CWORD'"
    COMP_POINT="${#COMP_LINE}" # index of cursor position
    #echo "COMP_POINT: '$COMP_POINT'"

    source dotnet-suggest-shim.bash

    local cmd=${COMP_WORDS[0]}

    local output=($(complete -p "$cmd"))
    local func=$(get_completion_function_name_from_complete_output "${output[@]}")

    #_dotnet_bash_complete
    "$func"

    echo "${COMPREPLY[@]}" > get_completions_result.txt
}

get_completions "$@"