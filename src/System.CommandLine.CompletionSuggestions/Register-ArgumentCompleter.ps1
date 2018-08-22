$availableToComplete = (dotnet-suggest list) | Out-String
$availableToCompleteArray = $availableToComplete.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries) |  ForEach-Object {$_.Trim() }

Register-ArgumentCompleter -Native -CommandName $availableToCompleteArray -ScriptBlock {
    param($commandName, $wordToComplete, $cursorPosition)
    $fullpath = (Get-Command $wordToComplete.CommandElements[0]).Source
    $result = (dotnet-suggest -p $cursorPosition -e $fullpath $wordToComplete.CommandElements) | Out-String
    $result.Trim() | ForEach-Object {
        [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
    }
}
