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
                    case build
                        set state 1
                    case --config
                        set i (math $i + 1)
                    case --framework -f
                        set -l skip_max 3
                        set -l skipped 0
                        while test $skipped -lt $skip_max -a (math $i + 1) -le (count $tokens)
                            set -l next $tokens[(math $i + 1)]
                            if string match -q -- '-*' $next
                                break
                            end
                            set i (math $i + 1)
                            set skipped (math $skipped + 1)
                        end
                    case --sources
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
                    case --config
                        if test $values_after -lt 1
                            printf '%s\n' 'debug'
                            printf '%s\n' 'release'
                            return
                        end
                    case --framework -f
                        if test $values_after -lt 3
                            printf '%s\n' 'net10.0'
                            printf '%s\n' 'net8.0'
                            printf '%s\n' 'net9.0'
                            return
                        end
                    case --sources
                        return
                end
        end
    end
    
    switch $state
        case 0
            printf '%s\n' 'build'
            printf '%s\n' '--config'
            printf '%s\n' '--framework'
            printf '%s\n' '-f'
            printf '%s\n' '--sources'
        case 1
    end
end

complete -c mycommand -f -a '(_mycommand)'
