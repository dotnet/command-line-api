# fish completions for mycommand

function _mycommand
    set -l tokens (commandline -opc)
    set -l current_token (commandline -ct)
    set -a tokens "$current_token"
    set -l state 0
    set -l position 0
    set -l end_options 0
    set -l capture_started 0
    set -l capture 0
    set -l pending 0
    set -l remaining 0
    set -l first_value 0
    set -l greedy 0
    set -l boolean 0
    set -l value_prefix ''
    set -l i 2
    while test $i -le (count $tokens)
        set -l word "$tokens[$i]"
        set -l current 0
        if test $i -eq (count $tokens)
            set current 1
        end
        set -l kind argument
        set -l next_state $state
        set -l option_id 0
        set -l maximum 0
        set -l option_greedy 0
        set -l option_boolean 0
        set -l inline 0
        set -l option_names
        set -l option_ids
        set -l option_maxima
        set -l option_greedies
        set -l option_booleans
        set -l option_name (string replace -r '[:=].*$' '' -- "$word")
        if test $end_options -eq 0
            switch $state
                case 0
                    set option_names '--name'
                    set option_ids 1
                    set option_maxima 1
                    set option_greedies 1
                    set option_booleans 0
                    switch $word
                        case 'subcommand'
                            set kind command
                            set next_state 1
                    end
                case 1
                    set option_names
                    set option_ids
                    set option_maxima
                    set option_greedies
                    set option_booleans
                    switch $word
                    end
            end
            if test "$kind" != command
                set -l index (contains -i -- "$word" $option_names)
                if test (count $index) -eq 0
                    set index (contains -i -- "$option_name" $option_names)
                    if test (count $index) -gt 0
                        set inline 1
                    end
                end
                if test (count $index) -gt 0
                    set kind option
                    set option_id $option_ids[$index]
                    set maximum $option_maxima[$index]
                    set option_greedy $option_greedies[$index]
                    set option_boolean $option_booleans[$index]
                else if test $end_options -eq 0; and string match -qr '^-[^-:=].+' -- "$word"
                    if not test $pending -gt 0 -a $first_value -eq 1 -a $greedy -eq 1
                        set -l bundle
                        set -l j 2
                        while test $j -le (string length -- "$word")
                            set -l short -(string sub -s $j -l 1 -- "$word")
                            set index (contains -i -- "$short" $option_names)
                            if test (count $index) -eq 0
                                if test (count $bundle) -gt 0
                                    set -l rest (string sub -s $j -- "$word" | string replace -r '^[:=]' '')
                                    set bundle[-1] "$bundle[-1]=$rest"
                                    if test $current -eq 1
                                        set value_prefix (string sub -l (math $j - 1) -- "$word")
                                    end
                                end
                                break
                            end
                            set -a bundle "$short"
                            set -l rest (string sub -s (math $j + 1) -- "$word")
                            if test -n "$rest"; and begin; test $option_greedies[$index] -eq 1; or string match -qr '^[:=]' -- "$rest"; end
                                set -l prefix_length $j
                                if string match -qr '^[:=]' -- "$rest"
                                    set prefix_length (math $j + 1)
                                end
                                set rest (string replace -r '^[:=]' '' -- "$rest")
                                set bundle[-1] "$short=$rest"
                                if test $current -eq 1
                                    set value_prefix (string sub -l $prefix_length -- "$word")
                                end
                                break
                            end
                            set j (math $j + 1)
                        end
                        if test (count $bundle) -gt 0
                            set -l before (math $i - 1)
                            set -l after (math $i + 1)
                            set tokens $tokens[1..$before] $bundle $tokens[$after..-1]
                            continue
                        end
                    end
                end
            end
        end
        set capture 0
        switch $state
        end
        if string match -q -- -- "$word"; and test $end_options -eq 0
            if test $current -eq 1
                set pending 0
                break
            end
            set end_options 1
            if test $capture_started -eq 0 -o $capture -eq 0
                set pending 0
                set i (math $i + 1)
                continue
            end
        end
        if test $pending -gt 0
            set -l consume 0
            if test "$kind" = argument; or test $first_value -eq 1 -a $greedy -eq 1 -a $inline -eq 0
                set consume 1
            end
            if test $boolean -eq 1
                if test $current -eq 0; and not string match -qi -r '^(true|false)$' -- "$word"
                    set consume 0
                else if test $current -eq 1; and not string match -qi -- "$word*" true false
                    set consume 0
                end
            end
            if test $consume -eq 1
                if test $current -eq 1
                    break
                end
                set remaining (math $remaining - 1)
                set first_value 0
                if test $remaining -eq 0
                    set pending 0
                end
                set i (math $i + 1)
                continue
            end
            set pending 0
        end
        if test $capture -eq 1
            set kind argument
            set capture_started 1
        end
        if test "$kind" = option -a $maximum -gt 0
            if test $current -eq 0 -o $inline -eq 1
                set pending $option_id
                set remaining $maximum
                set first_value 1
                set greedy $option_greedy
                set boolean $option_boolean
                if test $inline -eq 1 -a $current -eq 1; and test -z "$value_prefix"
                    set value_prefix (string replace -r '^([^:=]*[:=]).*$' '$1' -- "$word")
                end
                if test $inline -eq 1 -a $current -eq 0; and string match -q -r '[:=].+' -- "$word"
                    set remaining (math $remaining - 1)
                    set first_value 0
                    if test $boolean -eq 1; and not string match -qi -r '[:=](true|false)$' -- "$word"
                        set remaining 0
                        set position (math $position + 1)
                    end
                    if test $remaining -eq 0
                        set pending 0
                    end
                end
            end
        end
        if test $current -eq 1
            break
        end
        switch $kind
            case command
                set state $next_state
                set position 0
                set capture_started 0
            case argument
                set position (math $position + 1)
                if test $capture -eq 1 -a $inline -eq 1; and string match -q -r '[:=].+' -- "$word"
                    set position (math $position + 1)
                end
            case option
                if test $maximum -eq 0 -a $inline -eq 1; and string match -q -r '[:=].+' -- "$word"
                    set position (math $position + 1)
                end
        end
        set i (math $i + 1)
    end

    if test $pending -gt 0
        switch $pending
            case 1
        end
        return
    end

    switch $state
        case 0
            if test $end_options -eq 0 -a $capture -eq 0
                printf '%s\n' 'subcommand'
                printf '%s\n' '--name'
            end
        case 1
            if test $end_options -eq 0 -a $capture -eq 0
            end
    end
end

complete -c mycommand -f -a '(_mycommand)'
