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
