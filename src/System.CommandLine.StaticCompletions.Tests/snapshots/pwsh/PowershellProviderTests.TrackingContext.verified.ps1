using namespace System.Management.Automation
using namespace System.Management.Automation.Language

Register-ArgumentCompleter -Native -CommandName 'scltest' -ScriptBlock {
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
                @{ Maximum = 1; Capture = $false }
                @{ Maximum = 100000; Capture = $false }
            )
        }
        @{
            Commands = [System.Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)
            Options = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
            Arguments = @(
            )
        }
    )
    $states[0].Commands.Add('scltest', 0)
    $states[0].Commands.Add('child', 1)
    $states[0].Commands.Add('c', 1)
    $option = @{ Name = '--switch'; Maximum = 1; Greedy = $false; Boolean = $true }
    $states[0].Options.Add('--switch', $option)
    $states[0].Options.Add('-s', $option)
    $option = @{ Name = '-a'; Maximum = 1; Greedy = $false; Boolean = $true }
    $states[0].Options.Add('-a', $option)
    $option = @{ Name = '-A'; Maximum = 1; Greedy = $false; Boolean = $true }
    $states[0].Options.Add('-A', $option)
    $option = @{ Name = '--name'; Maximum = 1; Greedy = $true; Boolean = $false }
    $states[0].Options.Add('--name', $option)
    $states[0].Options.Add('-n', $option)
    $states[0].Options.Add('/name', $option)
    $option = @{ Name = '--optional'; Maximum = 1; Greedy = $false; Boolean = $false }
    $states[0].Options.Add('--optional', $option)
    $option = @{ Name = '--values'; Maximum = 3; Greedy = $true; Boolean = $false }
    $states[0].Options.Add('--values', $option)
    $option = @{ Name = '--single'; Maximum = 1; Greedy = $false; Boolean = $false }
    $states[0].Options.Add('--single', $option)
    $option = @{ Name = '--many'; Maximum = 100000; Greedy = $true; Boolean = $false }
    $states[0].Options.Add('--many', $option)
    $option = @{ Name = '--secret'; Maximum = 1; Greedy = $true; Boolean = $false }
    $states[0].Options.Add('--secret', $option)
    $states[1].Commands.Add('child', 1)
    $states[1].Commands.Add('c', 1)
    $states[1].Commands.Add('nested', 2)
    $option = @{ Name = '--child-only'; Maximum = 1; Greedy = $false; Boolean = $true }
    $states[1].Options.Add('--child-only', $option)
    $option = @{ Name = '--name'; Maximum = 1; Greedy = $true; Boolean = $false }
    $states[1].Options.Add('--name', $option)
    $states[1].Options.Add('-n', $option)
    $states[1].Options.Add('/name', $option)
    $states[2].Commands.Add('nested', 2)
    $option = @{ Name = '--nested-only'; Maximum = 1; Greedy = $false; Boolean = $true }
    $states[2].Options.Add('--nested-only', $option)
    $option = @{ Name = '--name'; Maximum = 1; Greedy = $true; Boolean = $false }
    $states[2].Options.Add('--name', $option)
    $states[2].Options.Add('-n', $option)
    $states[2].Options.Add('/name', $option)
    # Register-ArgumentCompleter passes the current word, the parsed command AST, and an absolute
    # cursor offset. CommandElements includes the executable, so tracking starts after that element.
    # First classify completed elements into parser-like tokens. The second pass assigns their values.
    $tokens = [System.Collections.Generic.List[object]]::new()
    $tokenState = 0
    $previousOption = $null
    $endOfOptions = $false
    $inDirectives = $true
    foreach ($element in $commandAst.CommandElements | Select-Object -Skip 1) {
        # Do not consume the element being completed or any element after the cursor.
        # If the cursor is inside an element, keep only the prefix for candidate filtering.
        if ($element.Extent.EndOffset -ge $cursorPosition) {
            if ($element.Extent.StartOffset -lt $cursorPosition) {
                $length = [Math]::Min($wordToComplete.Length, $cursorPosition - $element.Extent.StartOffset)
                $wordToComplete = $wordToComplete.Substring(0, $length)
            }
            break
        }
        # Use the unquoted value of a PowerShell string literal. For other AST nodes, use their text
        # without evaluating expressions, variables, or substitutions.
        $word = if ($element -is [StringConstantExpressionAst]) { $element.Value } else { $element.Extent.Text }
        $context = $states[$tokenState]
        if ($endOfOptions) {
            $tokens.Add(@{ Kind = 'argument'; Value = $word })
            continue
        }
        # -ceq is case-sensitive. The end marker makes all later elements positional tokens.
        if ($word -ceq '--') {
            $endOfOptions = $true
            $previousOption = $null
            $tokens.Add(@{ Kind = 'end' })
            continue
        }
        # Directives are recognized only before the first non-directive element.
        if ($inDirectives -and $word -match '^\[[^:\]][^\]]*\]$') { continue }
        $inDirectives = $false

        # A greedy option classifies the next known name as an argument, even if it names a subcommand.
        # Otherwise, command aliases select a new tokenization state and option aliases select metadata.
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
            # Split an inline value, such as --name=value or -n:value, into option and argument tokens.
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
                # Expand a POSIX-style bundle such as -ab. A greedy option or an unknown character
                # after a recognized option makes the remainder an argument value.
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

    # Replay the tokens to find the parser context at the cursor. $tokenState above is only for
    # token recognition; $state can differ when a positional argument captures subsequent tokens.
    # Option counts are per occurrence. Argument counts and indices belong to the current command.
    $state = 0
    $pendingOption = $null
    $optionValueCount = 0
    $argumentIndex = 0
    $argumentValueCount = 0
    $endOfOptions = $false
    $capturing = $false
    foreach ($token in $tokens) {
        $arguments = $states[$state].Arguments
        # Skip positional arguments whose maximum arity is already filled.
        while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
            $argumentIndex++
            $argumentValueCount = 0
        }
        # CaptureRemainingTokens can consume command and option tokens after capture starts.
        # An end marker before capture starts keeps its normal meaning.
        $capture = $argumentIndex -lt $arguments.Count -and $arguments[$argumentIndex].Capture -and
            ($capturing -or ($token.Kind -ne 'end' -and ($token.Kind -eq 'argument' -or $argumentIndex -gt 0 -or $argumentValueCount -gt 0)))
        # A pending option consumes contiguous argument tokens up to its per-occurrence maximum.
        # Boolean options consume only explicit true or false values, ignoring case and whitespace.
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
    # Advance once more so the index identifies the positional argument at the cursor.
    $arguments = $states[$state].Arguments
    while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
        $argumentIndex++
        $argumentValueCount = 0
    }
    # Keep the option and positional context for later completion features. For an inline value,
    # separate the option prefix from the value prefix without treating the current word as consumed.
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
                [CompletionResult]::new('--switch', '--switch', [CompletionResultType]::ParameterName, "--switch")
                [CompletionResult]::new('--switch', '-s', [CompletionResultType]::ParameterName, "--switch")
                [CompletionResult]::new('-a', '-a', [CompletionResultType]::ParameterName, "-a")
                [CompletionResult]::new('-A ', '-A', [CompletionResultType]::ParameterName, "-A ")
                [CompletionResult]::new('--name', '--name', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('--name', '-n', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('--name', '/name', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('--optional', '--optional', [CompletionResultType]::ParameterName, "--optional")
                [CompletionResult]::new('--values', '--values', [CompletionResultType]::ParameterName, "--values")
                [CompletionResult]::new('--single', '--single', [CompletionResultType]::ParameterName, "--single")
                [CompletionResult]::new('--many', '--many', [CompletionResultType]::ParameterName, "--many")
                [CompletionResult]::new('child', 'child', [CompletionResultType]::ParameterValue, "Child command")
                [CompletionResult]::new('child', 'c', [CompletionResultType]::ParameterValue, "Child command")
            )
            $completions += $staticCompletions
            break
        }
        1 {
            $staticCompletions = @(
                [CompletionResult]::new('--child-only', '--child-only', [CompletionResultType]::ParameterName, "--child-only")
                [CompletionResult]::new('--name', '--name', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('--name', '-n', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('--name', '/name', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('nested', 'nested', [CompletionResultType]::ParameterValue, "nested")
            )
            $completions += $staticCompletions
            break
        }
        2 {
            $staticCompletions = @(
                [CompletionResult]::new('--nested-only', '--nested-only', [CompletionResultType]::ParameterName, "--nested-only")
                [CompletionResult]::new('--name', '--name', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('--name', '-n', [CompletionResultType]::ParameterName, "--name")
                [CompletionResult]::new('--name', '/name', [CompletionResultType]::ParameterName, "--name")
            )
            $completions += $staticCompletions
            break
        }
    }
    $completions | Where-Object -FilterScript { $_.CompletionText -like "$wordToComplete*" } | Sort-Object -Property ListItemText
}
