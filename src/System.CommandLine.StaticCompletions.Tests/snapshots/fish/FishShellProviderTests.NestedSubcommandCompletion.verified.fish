# fish completions for mycommand

function _mycommand
    set -l tokens (commandline -opc)
    
    set -l state 0
    set -l i 2
    while test $i -le (count $tokens)
        set -l word $tokens[$i]
        switch $state
            case 0
                switch $word
                    case subcommand
                        set state 1
                end
            case 1
                switch $word
                    case nested
                        set state 2
                end
        end
        set i (math $i + 1)
    end
    
    switch $state
        case 0
            printf '%s\n' 'subcommand'
        case 1
            printf '%s\n' 'nested'
        case 2
    end
end

complete -c mycommand -f -a '(_mycommand)'
