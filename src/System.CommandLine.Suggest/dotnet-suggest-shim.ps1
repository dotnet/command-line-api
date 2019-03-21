# dotnet suggest shell complete script start
$availableToComplete = (dotnet-suggest list) | Out-String
$availableToCompleteArray = $availableToComplete.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries) |  ForEach-Object {$_.Trim() }

Register-ArgumentCompleter -Native -CommandName $availableToCompleteArray -ScriptBlock {
    param($commandName, $wordToComplete, $cursorPosition)
    $fullpath = (Get-Command $wordToComplete.CommandElements[0]).Source

    $arguments = @('get', '-e', $fullpath, '--position', $cursorPosition, '--') + (,$wordToComplete.CommandElements)
    dotnet-suggest @arguments | ForEach-Object {
        [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
    }
}
$env:DOTNET_SUGGEST_SCRIPT_VERSION = "1.0.0"
# dotnet suggest shell complete script end