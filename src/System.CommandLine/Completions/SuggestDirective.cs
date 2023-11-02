using System.CommandLine.Invocation;

namespace System.CommandLine.Completions;

/// <summary>
/// Enables the use of the <c>[suggest]</c> directive which when specified in command line input short circuits normal command handling and writes a newline-delimited list of suggestions suitable for use by most shells to provide command line completions.
/// </summary>
/// <remarks>The <c>dotnet-suggest</c> tool requires the suggest directive to be enabled for an application to provide completions.</remarks>
public sealed class SuggestDirective : CliDirective
{
    private CliAction? _action;

    /// <inheritdoc />
    public SuggestDirective() : base("suggest")
    {
    }

    /// <inheritdoc />
    public override CliAction? Action
    {
        get => _action ??= new CompletionAction(this);
        set => _action = value ?? throw new ArgumentNullException(nameof(value));
    }

}