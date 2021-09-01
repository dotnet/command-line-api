# dotnet suggest shell start
if (Get-Command "dotnet-suggest" -errorAction SilentlyContinue)
{
    $availableToComplete = (dotnet-suggest list) | Out-String
    $availableToCompleteArray = $availableToComplete.Split([Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries)

    Register-ArgumentCompleter -Native -CommandName $availableToCompleteArray -ScriptBlock {
        param($wordToComplete, $commandAst, $cursorPosition)
        $fullpath = (Get-Command $commandAst.CommandElements[0]).Source

        $arguments = $commandAst.Extent.ToString().Replace('"', '\"')
        dotnet-suggest get -e $fullpath --position $cursorPosition -- "$arguments" | ForEach-Object {
            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
        }
    }    
}
else
{
    "Unable to provide System.CommandLine tab completion support unless the [dotnet-suggest] tool is first installed."
    "See the following for tool installation: https://www.nuget.org/packages/dotnet-suggest"
}

$env:DOTNET_SUGGEST_SCRIPT_VERSION = "1.0.2"
# dotnet suggest script end
