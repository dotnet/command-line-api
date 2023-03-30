namespace System.CommandLine;

/// <summary>
/// Provides extension methods for <see cref="CliCommand" />.
/// </summary>
public static class CommandExtensions
{
    private const string _messageForWhenGeneratorIsNotInUse =
            "This overload should not be called. You should reference the System.CommandLine.Generator package which will generate a more specific overload for your delegate.";

    /// <summary>
    /// Sets a command handler.
    /// </summary>
    /// <remarks>Currently, this method only works with C# source generators.</remarks>
    /// <param name="command">The command on which to set the handler.</param>
    /// <param name="delegate">A delegate implementing the handler for the command.</param>
    /// <param name="symbols">The symbols used to bind the handler's parameters.</param>
    public static void SetHandler<TDelegate>(
        this CliCommand command,
        TDelegate @delegate,
        params CliSymbol[] symbols)
    {
        throw new InvalidOperationException(_messageForWhenGeneratorIsNotInUse);
    }
}

