# dotnet suggest shell start
# Builds the same list as `dotnet-suggest list` without starting a process, to keep shell startup fast.
$dotnetSuggestHome = if ($env:DOTNET_CLI_HOME) { $env:DOTNET_CLI_HOME } else { $HOME }
$availableToCompleteArray = @(
    try { [System.IO.Directory]::GetFiles("$dotnetSuggestHome/.dotnet/tools") } catch { }
    try { [System.IO.File]::ReadAllLines("$HOME/.dotnet-suggest-registration.txt") } catch { }
)
$availableToCompleteArray = @($availableToCompleteArray -replace '^\s+|\s+$' -replace '^.*[\\/]' -replace '\.[^.]*$' -ne '')
$availableToCompleteArray += @($availableToCompleteArray -clike 'dotnet-*' -creplace '^dotnet-', 'dotnet ')

if ($availableToCompleteArray -contains "dotnet-suggest" -or (Get-Command "dotnet-suggest" -errorAction SilentlyContinue))
{
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

$env:DOTNET_SUGGEST_SCRIPT_VERSION = "1.0.3"
# dotnet suggest script end
