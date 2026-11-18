using namespace System.Management.Automation
using namespace System.Management.Automation.Language

Register-ArgumentCompleter -Native -CommandName 'mycommand' -ScriptBlock {
    param($wordToComplete, $commandAst, $cursorPosition)

    $states = @(
        @{
            Commands = [System.Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)
            Options = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
            Arguments = @(
            )
        }
        @{
            Commands = [System.Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)
            Options = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
            Arguments = @(
            )
        }
        @{
            Commands = [System.Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)
            Options = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
            Arguments = @(
            )
        }
    )
    $states[0].Commands.Add('mycommand', 0)
    $states[0].Commands.Add('subcommand', 1)
    $states[1].Commands.Add('subcommand', 1)
    $states[1].Commands.Add('nested', 2)
    $states[2].Commands.Add('nested', 2)
    $tokens = [System.Collections.Generic.List[object]]::new()
    $tokenState = 0
    $previousOption = $null
    $endOfOptions = $false
    $inDirectives = $true
    foreach ($element in $commandAst.CommandElements | Select-Object -Skip 1) {
        if ($element.Extent.EndOffset -ge $cursorPosition) {
            if ($element.Extent.StartOffset -lt $cursorPosition) {
                $length = [Math]::Min($wordToComplete.Length, $cursorPosition - $element.Extent.StartOffset)
                $wordToComplete = $wordToComplete.Substring(0, $length)
            }
            break
        }
        $word = if ($element -is [StringConstantExpressionAst]) { $element.Value } else { $element.Extent.Text }
        $context = $states[$tokenState]
        if ($endOfOptions) {
            $tokens.Add(@{ Kind = 'argument'; Value = $word })
            continue
        }
        if ($word -ceq '--') {
            $endOfOptions = $true
            $previousOption = $null
            $tokens.Add(@{ Kind = 'end' })
            continue
        }
        if ($inDirectives -and $word -match '^\[[^:\]][^\]]*\]$') { continue }
        $inDirectives = $false

        $known = $context.Commands.ContainsKey($word) -or $context.Options.ContainsKey($word)
        if ($known -and $previousOption -and $previousOption.Greedy) {
            $tokens.Add(@{ Kind = 'argument'; Value = $word })
            $previousOption = $null
        } elseif ($context.Commands.ContainsKey($word)) {
            $nextState = $context.Commands[$word]
            if ($nextState -eq $tokenState) {
                $tokens.Add(@{ Kind = 'argument'; Value = $word })
            } else {
                $tokenState = $nextState
                $tokens.Add(@{ Kind = 'command'; State = $tokenState })
            }
            $previousOption = $null
        } elseif ($context.Options.ContainsKey($word)) {
            $previousOption = $context.Options[$word]
            $tokens.Add(@{ Kind = 'option'; Option = $previousOption })
        } else {
            $separator = $word.IndexOfAny([char[]]'=:')
            $name = if ($separator -ge 0) { $word.Substring(0, $separator) } else { $word }
            if ($separator -ge 0 -and $context.Options.ContainsKey($name)) {
                $previousOption = $context.Options[$name]
                $tokens.Add(@{ Kind = 'option'; Option = $previousOption })
                if ($separator -lt $word.Length - 1) {
                    $tokens.Add(@{ Kind = 'argument'; Value = $word.Substring($separator + 1) })
                    $previousOption = $null
                }
            } elseif ($word.Length -gt 2 -and $word[0] -eq '-' -and $word[1] -ne '-' -and
                      $word[2] -notin ':', '=' -and -not ($previousOption -and $previousOption.Greedy)) {
                $bundle = [System.Collections.Generic.List[object]]::new()
                for ($j = 1; $j -lt $word.Length; $j++) {
                    $name = '-' + $word[$j]
                    if (-not $context.Options.ContainsKey($name)) {
                        if ($bundle.Count -gt 0) {
                            $bundle.Add(@{ Kind = 'argument'; Value = $word.Substring($j) })
                        }
                        break
                    }
                    $option = $context.Options[$name]
                    $bundle.Add(@{ Kind = 'option'; Option = $option })
                    if ($j -lt $word.Length - 1 -and ($option.Greedy -or $word[$j + 1] -in ':', '=')) {
                        $start = $j + 1
                        if ($word[$start] -in ':', '=') { $start++ }
                        $bundle.Add(@{ Kind = 'argument'; Value = $word.Substring($start) })
                        break
                    }
                }
                if ($bundle.Count -gt 0) {
                    $tokens.AddRange($bundle)
                    $last = $bundle[$bundle.Count - 1]
                    $previousOption = if ($last.Kind -eq 'option') { $last.Option } else { $null }
                } else {
                    $tokens.Add(@{ Kind = 'argument'; Value = $word })
                    $previousOption = $null
                }
            } else {
                $tokens.Add(@{ Kind = 'argument'; Value = $word })
                $previousOption = $null
            }
        }
    }

    $state = 0
    $pendingOption = $null
    $optionValueCount = 0
    $argumentIndex = 0
    $argumentValueCount = 0
    $endOfOptions = $false
    $capturing = $false
    foreach ($token in $tokens) {
        $arguments = $states[$state].Arguments
        while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
            $argumentIndex++
            $argumentValueCount = 0
        }
        $capture = $argumentIndex -lt $arguments.Count -and $arguments[$argumentIndex].Capture -and
            ($capturing -or ($token.Kind -ne 'end' -and ($token.Kind -eq 'argument' -or $argumentIndex -gt 0 -or $argumentValueCount -gt 0)))
        if ($pendingOption -and $token.Kind -eq 'argument') {
            if (-not $pendingOption.Boolean -or $token.Value.Trim() -imatch '^(true|false)$') {
                $optionValueCount++
                if ($optionValueCount -ge $pendingOption.Maximum) { $pendingOption = $null }
                continue
            }
        }
        $pendingOption = $null
        if ($capture) {
            $capturing = $true
            $argumentValueCount++
        } elseif ($token.Kind -eq 'end') {
            $endOfOptions = $true
        } elseif ($token.Kind -eq 'command') {
            $state = $token.State
            $argumentIndex = 0
            $argumentValueCount = 0
            $capturing = $false
        } elseif ($token.Kind -eq 'option') {
            $optionValueCount = 0
            if ($token.Option.Maximum -gt 0) { $pendingOption = $token.Option }
        } elseif ($argumentIndex -lt $arguments.Count) {
            $argumentValueCount++
        }
    }
    $arguments = $states[$state].Arguments
    while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
        $argumentIndex++
        $argumentValueCount = 0
    }
    $completionOption = $pendingOption
    $valuePrefix = ''
    $valueToComplete = $wordToComplete
    $separator = $wordToComplete.IndexOfAny([char[]]'=:')
    if (-not $endOfOptions -and -not $capturing -and $separator -ge 0) {
        $name = $wordToComplete.Substring(0, $separator)
        if ($states[$state].Options.ContainsKey($name) -and $states[$state].Options[$name].Maximum -gt 0) {
            $completionOption = $states[$state].Options[$name]
            $valuePrefix = $wordToComplete.Substring(0, $separator + 1)
            $valueToComplete = $wordToComplete.Substring($separator + 1)
        }
    }
    $completions = @()
    switch ($state) {
        0 {
            $staticCompletions = @(
                [CompletionResult]::new('subcommand', 'subcommand', [CompletionResultType]::ParameterValue, "subcommand")
            )
            $completions += $staticCompletions
            break
        }
        1 {
            $staticCompletions = @(
                [CompletionResult]::new('nested', 'nested', [CompletionResultType]::ParameterValue, "nested")
            )
            $completions += $staticCompletions
            break
        }
        2 {
            $staticCompletions = @(
            )
            $completions += $staticCompletions
            break
        }
    }
    $completions | Where-Object -FilterScript { $_.CompletionText -like "$wordToComplete*" } | Sort-Object -Property ListItemText
}
