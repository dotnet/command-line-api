using namespace System.Management.Automation
using namespace System.Management.Automation.Language

Register-ArgumentCompleter -Native -CommandName 'mycommand' -ScriptBlock {
    param($wordToComplete, $commandAst, $cursorPosition)

    $commandElements = $commandAst.CommandElements
    $command = @(
        'mycommand'
        for ($i = 1; $i -lt $commandElements.Count; $i++) {
            $element = $commandElements[$i]
            if ($element -isnot [StringConstantExpressionAst] -or
                $element.StringConstantType -ne [StringConstantType]::BareWord -or
                $element.Value.StartsWith('-') -or
                $element.Value -eq $wordToComplete) {
                break
            }
            $element.Value
        }) -join ';'

    $completions = @()
    switch ($command) {
        'mycommand' {
            $staticCompletions = @(
                [CompletionResult]::new('--option', '--option', [CompletionResultType]::ParameterName, '--option')
                [CompletionResult]::new('$(command)', '$(command)', [CompletionResultType]::ParameterValue, '$(command)')
                [CompletionResult]::new('$variable', '$variable', [CompletionResultType]::ParameterValue, '$variable')
                [CompletionResult]::new('back\slash', 'back\slash', [CompletionResultType]::ParameterValue, 'back\slash')
                [CompletionResult]::new('insert '' " $ ` \', 'display '' " $ ` \', [CompletionResultType]::ParameterValue, 'documentation '' " $ ` \ second line')
                [CompletionResult]::new('double"quote', 'double"quote', [CompletionResultType]::ParameterValue, 'double"quote')
                [CompletionResult]::new('line break', 'line break', [CompletionResultType]::ParameterValue, 'line break')
                [CompletionResult]::new('semi;pipe|amp&parens()brackets[]colon:glob*?', 'semi;pipe|amp&parens()brackets[]colon:glob*?', [CompletionResultType]::ParameterValue, 'semi;pipe|amp&parens()brackets[]colon:glob*?')
                [CompletionResult]::new('single''quote', 'single''quote', [CompletionResultType]::ParameterValue, 'single''quote')
                [CompletionResult]::new('tab value', 'tab value', [CompletionResultType]::ParameterValue, 'tab value')
                [CompletionResult]::new('value with spaces', 'value with spaces', [CompletionResultType]::ParameterValue, 'value with spaces')
                [CompletionResult]::new('`command`', '`command`', [CompletionResultType]::ParameterValue, '`command`')
            )
            $completions += $staticCompletions
            break
        }
    }
    $escapedWordToComplete = [WildcardPattern]::Escape($wordToComplete)
    $completions | Where-Object -FilterScript { $_.CompletionText -like "$escapedWordToComplete*" } | Sort-Object -Property ListItemText
}
