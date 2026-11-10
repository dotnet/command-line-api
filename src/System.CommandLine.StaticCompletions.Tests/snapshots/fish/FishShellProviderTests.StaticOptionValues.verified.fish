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
                    case --verbosity
                        set i (math $i + 1)
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
                    case --verbosity
                        if test $values_after -lt 1
                            printf '%s\n' 'detailed'
                            printf '%s\n' 'diagnostic'
                            printf '%s\n' 'minimal'
                            printf '%s\n' 'normal'
                            printf '%s\n' 'quiet'
                            return
                        end
                end
        end
    end
    
    switch $state
        case 0
            printf '%s\n' '--verbosity'
    end
end

complete -c mycommand -f -a '(_mycommand)'
