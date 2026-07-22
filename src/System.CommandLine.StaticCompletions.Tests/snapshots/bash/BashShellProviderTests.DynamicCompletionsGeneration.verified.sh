#!/usr/bin/env bash
_mycommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts=''\''--name'\'''
    opts="$opts $(${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null | tr '\n' ' ')" 
    
    if [[ $COMP_CWORD == "1" ]]; then
        while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
        return
    fi
    
    case $prev in
        --name)
            while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "(${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null | tr '\n' ' ')" -- "$cur")
            return
        ;;
    esac
    
    while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
}



complete -F _mycommand mycommand