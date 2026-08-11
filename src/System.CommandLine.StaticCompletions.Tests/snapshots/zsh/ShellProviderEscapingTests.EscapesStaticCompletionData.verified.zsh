#compdef mycommand

autoload -U is-at-least

_mycommand() {
    typeset -A opt_args
    typeset -a _arguments_options
    local ret=1

    if is-at-least 5.2; then
        _arguments_options=(-s -S -C)
    else
        _arguments_options=(-s -C)
    fi

    local context curcontext="$curcontext" state state_descr line
    _arguments "${_arguments_options[@]}" : \
        '--option=[]: :(('\''$(command)'\''\:"\$(command)" '\''$variable'\''\:"\$variable" '\''back\slash'\''\:"back\\slash" '\''insert '\''\'\'''\'' " $ ` \'\''\:"documentation '\'' \" \$ \` \\ second line" '\''double"quote'\''\:"double\"quote" '\''line break'\''\:"line break" '\''semi;pipe|amp&parens()brackets[]colon:glob*?'\''\:"semi;pipe|amp&parens()brackets\[\]colon\:glob*?" '\''single'\''\'\'''\''quote'\''\:"single'\''quote" '\''tab value'\''\:"tab value" '\''value with spaces'\''\:"value with spaces" '\''`command`'\''\:"\`command\`" ))' \
        ':argument:(('\''$(command)'\''\:"\$(command)" '\''$variable'\''\:"\$variable" '\''back\slash'\''\:"back\\slash" '\''insert '\''\'\'''\'' " $ ` \'\''\:"documentation '\'' \" \$ \` \\ second line" '\''double"quote'\''\:"double\"quote" '\''line break'\''\:"line break" '\''semi;pipe|amp&parens()brackets[]colon:glob*?'\''\:"semi;pipe|amp&parens()brackets\[\]colon\:glob*?" '\''single'\''\'\'''\''quote'\''\:"single'\''quote" '\''tab value'\''\:"tab value" '\''value with spaces'\''\:"value with spaces" '\''`command`'\''\:"\`command\`" ))' \
        && ret=0
    local original_args="mycommand ${line[@]}" 
}

(( $+functions[_mycommand_commands] )) ||
_mycommand_commands() {
    local commands; commands=()
    _describe -t commands 'mycommand commands' commands "$@"
}

if [ "$funcstack[1]" = "_mycommand" ]; then
    _mycommand "$@"
else
    compdef _mycommand mycommand
fi
