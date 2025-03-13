using System.Diagnostics;

namespace System.CommandLine;

internal static class DiagnosticsStrings
{
    internal const string LibraryNamespace = "System.CommandLine";
    internal const string ParseMethod = LibraryNamespace + ".Parse";
    internal const string InvokeMethod = LibraryNamespace + ".Invoke";
    internal const string InvokeType = nameof(InvokeType);
    internal const string Async = nameof(Async);
    internal const string Sync = nameof(Sync);
    internal const string ExitCode = nameof(ExitCode);
    internal const string Exception = nameof(Exception);
    internal const string Command = nameof(Command);
}

internal static class Activities
{
    internal static readonly ActivitySource ActivitySource = new ActivitySource(DiagnosticsStrings.LibraryNamespace);
}

internal static class ActivityExtensions
{

    /// <summary>
    /// Walks up the command tree to get the build the command name by prepending the parent command names to the 'leaf' command name.
    /// </summary>
    /// <param name="commandResult"></param>
    /// <returns>The full command name, like 'dotnet package add'.</returns>
    internal static string FullCommandName(this Parsing.CommandResult commandResult)
    {
        var command = commandResult.Command;
        var path = command.Name;

        while (commandResult.Parent is Parsing.CommandResult parent)
        {
            command = parent.Command;
            path = $"{command.Name} {path}";
            commandResult = parent;
        }

        return path;
    }
}