# Shell Completion Data Escaping

## Status

This document defines how static completion providers must handle completion data.

## Context

The static completions generator emits scripts for many different shells.
Each shell uses different quoting rules and can parse completion data more than once.

Completion sources can return data that the command-line author does not control.
For example, a source can return file names, database values, or values from an external service.
The generator must preserve this data as completion data and must not emit it as shell syntax.

This requirement applies when the generator resolves a completion source during script generation.
It also applies when a generated script resolves a dynamic completion source at run time.

## Trust boundary

Command-line authors control these values:

- Command names and aliases.
- Option names and aliases.
- Argument names.
- Symbol descriptions.

Authors must use values that are valid for each supported shell.
The generator does not treat malicious author-controlled symbol metadata as a security boundary.

Completion source output is outside this trust boundary.
The generator must sanitize and quote completion candidates, insertion text, and completion documentation.

## Goals

The implementation must:

- Preserve one completion item as one shell completion candidate.
- Prevent shell evaluation of completion source output.
- Prevent completion data from changing generated shell syntax.
- Prevent variable, command, path, tilde, and glob expansion.
- Preserve spaces, quotes, backslashes, and shell metacharacters.
- Use `CompletionItem.InsertText` as the inserted value when it is available.
- Use `CompletionItem.Label` when insertion text is not available.
- Keep completion descriptions separate from inserted values.

## Non-goals

The implementation does not:

- Make arbitrary command, option, or argument names valid in all shells.
- Treat author-controlled symbol names as untrusted input.
- Change a shell grammar to support an incompatible alias.
- Guarantee that all Unicode control characters are valid completion text.

For example, Zsh `_arguments` does not accept `/v` as an option switch.
Escaping `/v` does not make that alias valid.
A shell completions provider must omit aliases that its shell grammar cannot represent.

## Common completion normalization

Completion protocols commonly use lines and tabs as record separators.
A completion item must not contain these separators.

Before shell-specific quoting, replace these characters with one space:

- Carriage return followed by line feed.
- Line feed.
- Carriage return.
- Tab.

`NormalizeCompletionText` implements this rule for generated static candidates.

Descriptions must follow the same record-boundary rule.
A provider can use a separate description helper when its output format has additional constraints.

## Shell-specific encoding

Each provider must encode data at the last shell syntax boundary.
A shared generic shell-escape function is not sufficient.

### Bash

`compgen -W` parses its word-list argument as shell words.
The generated script also parses the assignment that contains that word list.

The Bash provider must:

1. Normalize each candidate.
2. Quote each candidate as one Bash single-quoted word.
3. Encode an apostrophe as `'\''`.
4. Join the quoted words with spaces.
5. Quote the complete word-list representation for the generated assignment.
6. Read `compgen` output line by line into `COMPREPLY`.

The provider must not use unquoted command substitution to construct `COMPREPLY`.
That operation splits candidates that contain spaces.

Dynamic completion output must use an equivalent record-preserving path.
It must not become unquoted input to a second shell evaluation.

### Zsh

Zsh `_arguments` evaluates some value expressions a second time.
Escaping only selected metacharacters is not sufficient.

The Zsh provider must:

1. Normalize each candidate.
2. Quote the complete candidate for the `_arguments` evaluation.
3. Quote that representation for the outer generated script.
4. Escape an apostrophe at each applicable single-quoted boundary.
5. Encode descriptions separately for their nested double-quoted context.
6. Escape description backslashes before quotes and other description metacharacters.

This design keeps pipes, control operators, globs, tildes, variables, and substitutions literal.

### Fish

The Fish provider emits candidates with `printf`.
Fish then consumes each output line as one candidate record.

The Fish provider must:

1. Normalize each candidate.
2. Pass each candidate as a separate single-quoted `printf` argument.
3. Escape backslashes and apostrophes for Fish single-quoted strings.
4. Normalize descriptions before producing `candidate<TAB>description` output.

The candidate must not contain a line feed or tab after normalization.

### PowerShell

The PowerShell provider constructs `CompletionResult` objects.

The PowerShell provider must:

1. Normalize completion text.
2. Emit each value as a PowerShell single-quoted string.
3. Encode an apostrophe as two apostrophes.
4. Normalize tooltip line endings.
5. Escape `$wordToComplete` with `WildcardPattern.Escape` before using `-like`.

The final rule makes typed `*`, `?`, and `[` characters literal prefix text.

### Nushell

The Nushell provider currently delegates completion to the application through `[suggest]`.
It does not embed completion source values in the generated script.

The provider must continue to consume dynamic results as records.
It must not evaluate a returned candidate as Nushell source.

## Static and dynamic completion paths

Static and dynamic describe when the provider resolves a completion source.
They do not define whether completion source output is trusted.

The same data rules apply to both paths:

- Treat each result as one record.
- Normalize record separators.
- Quote data for the destination shell context.
- Do not evaluate returned text as shell source.

Providers should share normalization behavior where shell protocols have the same constraints.
Providers must keep shell-specific quoting in the provider implementation.

## Test strategy

### Provider contract

The escaping contract test enumerates `CompletionsCommandParser.ShellProviders`.
This enumeration automatically adds a test case when a provider is registered.

The common payload set must include:

- Spaces.
- Apostrophes and double quotes.
- Backslashes.
- Variable and command substitution syntax.
- Backticks.
- Pipes and control operators.
- Parentheses, brackets, and colons.
- Glob characters.
- Carriage returns, line feeds, and tabs.
- Different label, insertion text, and documentation values.

Snapshots verify the generated shell representation.

### Execution tests

Snapshots do not prove that a shell preserves candidate boundaries.
Where a shell executable is available, tests should:

1. Generate the completion script.
2. Parse or source the script.
3. Trigger the completion path.
4. Capture the resulting candidates.
5. Compare each candidate with the normalized expected value.
6. Verify that substitution payloads create no side effects.

Zsh tests must exercise the `_arguments` evaluation layer.
Bash tests must exercise `compgen -W`.
PowerShell tests must include unmatched wildcard syntax such as `[`.

## Acceptance criteria

A provider satisfies this design when:

- Each uncontrolled completion result produces one candidate.
- The candidate equals the normalized insertion text.
- Completion documentation cannot change candidate boundaries.
- Shell metacharacters remain literal.
- No candidate causes command execution or shell expansion.
- Adding a provider automatically adds the common escaping contract test.
