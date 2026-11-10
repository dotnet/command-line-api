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
                    case --name
                        set i (math $i + 1)
                    case --items
                        while test (math $i + 1) -le (count $tokens)
                            set -l next $tokens[(math $i + 1)]
                            if string match -q -- '-*' $next
                                break
                            end
                            set i (math $i + 1)
                        end
                end
        end
        set i (math $i + 1)
    end
    
    set -l opt_index 0
    if test (count $tokens) -ge 2
        for j in (seq (count $tokens) -1 2)
            if string match -q -- '-*' $tokens[$j]
                set opt_index $j
                break
            end
        end
    end
    
    if test $opt_index -gt 0
        set -l opt $tokens[$opt_index]
        set -l values_after (math (count $tokens) - $opt_index)
        switch $state
            case 0
                switch $opt
                    case --items
                        printf '%s\n' 'a'
                        printf '%s\n' 'b'
                        printf '%s\n' 'c'
                        return
                    case --name
                        if test $values_after -lt 1
                            return
                        end
                end
        end
    end
    
    switch $state
        case 0
            printf '%s\n' 'subcommand'
            printf '%s\n' '--items'
            printf '%s\n' '--name'
        case 1
    end
end

complete -c mycommand -f -a '(_mycommand)'
