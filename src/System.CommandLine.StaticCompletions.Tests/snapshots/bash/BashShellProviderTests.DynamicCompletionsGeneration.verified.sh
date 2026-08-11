#!/usr/bin/env bash
_mycommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts=''\''--name'\'''
    
    if [[ $COMP_CWORD == "1" ]]; then
        while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
        while IFS= read -r completion; do [[ $completion == "$cur"* ]] && COMPREPLY+=("$completion"); done < <(${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null)
        return
    fi
    
    case $prev in
        --name)
            while IFS= read -r completion; do [[ $completion == "$cur"* ]] && COMPREPLY+=("$completion"); done < <(${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null)
            return
        ;;
    esac
    
    while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
    while IFS= read -r completion; do [[ $completion == "$cur"* ]] && COMPREPLY+=("$completion"); done < <(${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null)
}



complete -F _mycommand mycommand