# Proposal for error handling in Powderhouse

The same `Error` type will be used for parse errors and subsystem errors, including validation. It will also be available for use by invocations.

`ParseResult` will contain collection of `CliError` objects that are created during parsing. This will include both argument specific errors like type conversion and arity, and general errors like response file issues. Errors for a specific argument or option will be available from its `ValueResult`.

## API

* `CliDiagnosticDescriptor`
  *`string DiagnosticId`
  * `string DefaultMessage` (to be used in `String.Format()`)
  * `Uri HelpUri`
  * `Severity DefaultSeverity`

* `CliError`
  * `public string DiagnosticId`
  * `public Location Location` (which includes Symbol and Text to allow messages to be what user typed)
  * `public string? Message` (defaults to Diagnostic)
  * `public IEnumerable<string> data` (data for message string)
  * `public string AdditionalContextSpecificText` (name for clarity, adjust later)
  * `public Severity? Severity` (defaults to Diagnostic)

### `ErrorSubsystem`

Errors will be managed and reported via the ErrorSubsystem. This is to allow errors to be easily added by any subsystem. This might be preprocessing, execution, or optionally by an uncaught exception. Addition of errors by invocations is supported, to allow consistent reporting of errors.

The `ErrorSubsystem` will write errors to the console and be able to output in the anticipated format by MSBuild. We anticipate that community subsystems derived from this will supply fancier console output, and alternate output such as SARIF.

* `ErrorSubsystem`

  * `protected internal override bool GetIsActivated(ParseResult? parseResult)` // Any unreported errors or any not yet reported warnings?
  * `protected internal override CliExit Execute(PipelineContext pipelineContext)` // Display errors to console unless configured not to
  * `public IEnumerable<Error> Errors { get; }`
  * `public void AddError(Error error);`
  * `public IEnumerable<MSBuildError> MSBuildErrors { get; }`

_Question:_ How should we handle supplying MSBuild errors?

`Execute` will return a new `CliExit` instance with `Handled` set to `true` if errors were found. If `ErrorReporting` is configured not to output (because errors are only used via `MSBuildErrors`), `Execute` will still be run to set up for termination. If there are no errors, the previous CliExit code will be returned.

## Pipeline

Errors needs to be reported and the pipeline potentially terminated at several steps:

* Preprocessing
* ErrorReporting
* Parse
* ErrorReporting
* Help and Version
* ErrorReporting
* Validation and defaults
* ErrorReporting
* Invocation and TearDown
* ErrorReporting

To keep it simple, `ErrorReporting.ExecuteIfNeeded` will be called after every subsystem's `Initialize`, `CheckIfRequested`, `Execute` and `Teardown` call. This will need to avoid outputting the same error twice. This should handle both reporting warnings and calling subsystems that run even when terminating.

## Arity and required

Arity is interesting because it impacts parsing, but is really a validation concernn. IOW, it can be either a syntax or semantic problem. 

_The following understanding is preliminary and should be checked_

_Commands and options are semanticly different because commands can have multiple arguments, and options can have only one argument. Thus (in ParseOperation) commands stop when they reach the maximum arity and validation for maximum arity will always succeed and different errors would be thrown._

_Any extra tokens would result in different errors for commands and options. This makes sense, unless the current argument is the last argument, in which case option and command will have different messages for similar scenarios. We will not fix this apparent inconsistency as part of this issue, and probably not of this effort as we intend to preserve existing behavior._

_ArgumentLimitReached appears to be used primarily for completions. Retain it as part of the effort on Errors as it is unrelated and should be resolved with that._
