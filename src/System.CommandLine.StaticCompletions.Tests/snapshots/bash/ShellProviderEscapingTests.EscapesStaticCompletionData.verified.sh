#!/usr/bin/env bash
_mycommand() {

    cur="${COMP_WORDS[COMP_CWORD]}" 
    prev="${COMP_WORDS[COMP_CWORD-1]}" 
    COMPREPLY=()
    
    opts=''\''--option'\'' '\''$(command)'\'' '\''$variable'\'' '\''back\slash'\'' '\''insert '\''\'\'''\'' " $ ` \'\'' '\''double"quote'\'' '\''semi;pipe|amp&parens()brackets[]colon:glob*?'\'' '\''single'\''\'\'''\''quote'\'' '\''value with spaces'\'' '\''`command`'\'''
    
    if [[ $COMP_CWORD == "1" ]]; then
        while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
        return
    fi
    
    case $prev in
        --option)
            while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W ''\''$(command)'\'' '\''$variable'\'' '\''back\slash'\'' '\''insert '\''\'\'''\'' " $ ` \'\'' '\''double"quote'\'' '\''semi;pipe|amp&parens()brackets[]colon:glob*?'\'' '\''single'\''\'\'''\''quote'\'' '\''value with spaces'\'' '\''`command`'\''' -- "$cur")
            return
        ;;
    esac
    
    while IFS= read -r completion; do COMPREPLY+=("$completion"); done < <(compgen -W "$opts" -- "$cur")
}



complete -F _mycommand mycommand