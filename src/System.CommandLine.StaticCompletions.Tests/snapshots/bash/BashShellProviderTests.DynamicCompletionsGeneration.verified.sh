#!/usr/bin/env bash
_mycommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts="--name" 
    opts="$opts $(${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null | tr '\n' ' ')" 
    
    if [[ $COMP_CWORD == "1" ]]; then
        COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
        return
    fi
    
    case $prev in
        --name)
            COMPREPLY=( $(compgen -W "(${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null | tr '\n' ' ')" -- "$cur") )
            return
        ;;
    esac
    
    COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
}



complete -F _mycommand mycommand