_dotnet_bash_complete()
{
  local word=${COMP_WORDS[COMP_CWORD]}

  local completions=('$(dotnet-suggest get --executable "`type -p ${COMP_WORDS[0]}`" --position ${COMP_POINT} -- ${COMP_LINE})')

  COMPREPLY=( $(compgen -W "$completions" -- "$word") )
}

complete -f -F _dotnet_bash_complete `dotnet-suggest list`
