# fish completions for mycommand

function _mycommand
    set -l tokens (commandline -opc)
    
    set -l state 0
    set -l i 2
    while test $i -le (count $tokens)
        set -l word $tokens[$i]
        switch $state
        end
        set i (math $i + 1)
    end
    
    switch $state
        case 0
    end
end

complete -c mycommand -f -a '(_mycommand)'
