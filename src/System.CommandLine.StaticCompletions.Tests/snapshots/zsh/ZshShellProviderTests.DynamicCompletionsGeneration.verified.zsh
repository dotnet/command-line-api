#compdef my-app

autoload -U is-at-least

_my-app() {
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
        '--static=[]: :->suggest' \
        ':--dynamic:->suggest' \
        && ret=0
        case $state in
            (suggest)
                local completions=()
                local result=$(my-app "[suggest:${#original_args}]" "${original_args}" 2>/dev/null)
                for line in ${(f)result}; do
                    completions+=(${(q)line})
                done
                _describe 'completions' $completions && ret=0
            ;;
        esac
    local original_args="my-app ${line[@]}" 
}

(( $+functions[_my-app_commands] )) ||
_my-app_commands() {
    local commands; commands=()
    _describe -t commands 'my-app commands' commands "$@"
}

if [ "$funcstack[1]" = "_my-app" ]; then
    _my-app "$@"
else
    compdef _my-app my-app
fi
