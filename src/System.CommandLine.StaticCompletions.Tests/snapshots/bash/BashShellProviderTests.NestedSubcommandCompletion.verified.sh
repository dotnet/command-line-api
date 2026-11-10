#!/usr/bin/env bash
_mycommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts="subcommand" 
    
    if [[ $COMP_CWORD == "1" ]]; then
        COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
        return
    fi
    
    case ${COMP_WORDS[1]} in
        (subcommand)
            _mycommand_subcommand 2
            return
            ;;
            
    esac
    
    COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
}

_mycommand_subcommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts="nested" 
    
    if [[ $COMP_CWORD == "$1" ]]; then
        COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
        return
    fi
    
    case ${COMP_WORDS[$1]} in
        (nested)
            _mycommand_subcommand_nested $(($1+1))
            return
            ;;
            
    esac
    
    COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
}

_mycommand_subcommand_nested() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts="" 
    
    if [[ $COMP_CWORD == "$1" ]]; then
        COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
        return
    fi
    
    COMPREPLY=( $(compgen -W "$opts" -- "$cur") )
}



complete -F _mycommand mycommand