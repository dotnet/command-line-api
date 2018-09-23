$availableToComplete = (dotnet-suggest list) | Out-String
$availableToCompleteArray = $availableToComplete.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries) |  ForEach-Object {$_.Trim() }

Register-ArgumentCompleter -Native -CommandName $availableToCompleteArray -ScriptBlock {
    param($commandName, $wordToComplete, $cursorPosition)
    $fullpath = (Get-Command $wordToComplete.CommandElements[0]).Source
    $suggestionArray = ((dotnet-suggest get --position $cursorPosition --executable $fullpath -- $wordToComplete.CommandElements) | Out-String).Split([Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries)
    $suggestionArray | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }
}
