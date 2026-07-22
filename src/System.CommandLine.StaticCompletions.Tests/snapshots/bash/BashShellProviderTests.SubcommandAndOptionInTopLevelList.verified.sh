#!/usr/bin/env bash
_mycommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts=''\''subcommand'\'' '\''--name'\'''
    
    if [[ $COMP_CWORD == "1" ]]; then
        while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
        return
    fi
    
    case ${COMP_WORDS[1]} in
        (subcommand)
            _mycommand_subcommand 2
            return
            ;;
            
    esac
    
    while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
}

_mycommand_subcommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts=''
    
    if [[ $COMP_CWORD == "$1" ]]; then
        while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
        return
    fi
    
    while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
}



complete -F _mycommand mycommand