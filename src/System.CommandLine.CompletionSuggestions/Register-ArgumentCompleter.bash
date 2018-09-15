_dotnet_bash_complete()
{
  local word=${COMP_WORDS[COMP_CWORD]}

  local completions=("$(completion-suggestions -e ""`type -p ${COMP_WORDS[0]}`"" -p ${COMP_POINT} "${COMP_LINE}")")

  COMPREPLY=( $(compgen -W "$completions" -- "$word") )
}

complete -f -F _dotnet_bash_complete `completion-suggestions list`
