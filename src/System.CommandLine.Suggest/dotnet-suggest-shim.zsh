_dotnet_zsh_complete()
{
    # debug lines, uncomment to get state variables passed to this function
    # echo "\n\n\nstate:\t'$state'"
    # echo "line:\t'$line'"
    # echo "words:\t$words"

    # Get full path to script because dotnet-suggest needs it
    # NOTE: this requires a command registered with dotnet-suggest be
    # on the PATH
    full_path=`which ${words[1]}` # zsh arrays are 1-indexed
    # Get the full line
    # $words array when quoted like this gets expanded out into the full line
    full_line="$words"

    # Get the completion results, will be newline-delimited
    completions=$(dotnet suggest get --executable "$full_path" -- "$full_line")
    # explode the completions by linefeed instead of by spaces into the descriptions for the
    # _values helper function.
    
    exploded=(${(f)completions})
    # for later - once we have descriptions from dotnet suggest, we can stitch them
    # together like so:
    # described=()
    # for i in {1..$#exploded}; do
    #     argument="${exploded[$i]}"
    #     description="hello description $i"
    #     entry=($argument"["$description"]")
    #     described+=("$entry")
    # done
    _values 'suggestions' $exploded
}

# apply this function to each command the dotnet-suggest knows about
compdef _dotnet_zsh_complete $(dotnet-suggest list)

export DOTNET_SUGGEST_SCRIPT_VERSION="1.0.0"
