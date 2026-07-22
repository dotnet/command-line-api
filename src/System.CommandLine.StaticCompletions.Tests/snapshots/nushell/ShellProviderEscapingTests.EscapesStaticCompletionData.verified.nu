# nushell completions for mycommand
# save this file and `source` it from your nushell config

def "nu-complete mycommand" [context: string] {
    ^mycommand $"[suggest:($context | str length)]" $context | lines
}

export extern "mycommand" [
    ...command: string@"nu-complete mycommand"
]