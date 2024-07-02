using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

namespace System.CommandLine.Hosting.Tests;

internal static class CliCommandHandlerExtensions
{
    public static CliCommand UseCommandHandler<THandler>(
        this CliCommand command
        )
        where THandler : CliAction
    {
        _ = command ?? throw new ArgumentNullException(nameof(command));
        command.Action = CommandHandler.Create(
            typeof(THandler).GetMethod(nameof(AsynchronousCliAction.InvokeAsync))
            );
        return command;
    }
}
