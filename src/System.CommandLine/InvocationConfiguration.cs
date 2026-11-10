using System.CommandLine.Invocation;
using System.IO;
using System.Threading;

namespace System.CommandLine;

public class InvocationConfiguration
{
    private TextWriter? _output, _error;

    /// <summary>
    /// Gets or sets a value that indicates whether a default exception handler catches any unhandled exceptions thrown during invocation.
    /// </summary>
    /// <value><see langword="true"/> if a default exception handler catches any unhandled exceptions thrown during invocation. The default is <see langword="true"/>.</value>
    public bool EnableDefaultExceptionHandler { get; set; } = true;

    /// <summary>
    /// Gets or sets a time span that enables signaling and handling of process termination (Ctrl+C, SIGINT, SIGTERM) via a <see cref="CancellationToken"/>
    /// that can be passed to a <see cref="CommandLineAction"/> during invocation.
    /// </summary>
    /// <value>The default is two seconds.</value>
    /// <remarks>
    /// If this property is set to <see langword="null" />, the termination request isn't handled by System.CommandLine. In that case, the process is terminated immediately unless some other part of the program adds a handler.
    /// </remarks>
    public TimeSpan? ProcessTerminationTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets the standard output.
    /// </summary>
    /// <value>The default is set to <see cref="Console.Out"/>.</value>
    /// <remarks>
    /// The standard output is used by Help and other facilities that write non-error information.
    /// For testing purposes, it can be set to a new instance of <see cref="StringWriter"/>.
    /// If you want to disable the output, set it to <see cref="TextWriter.Null"/>.
    /// </remarks>
    public TextWriter Output
    {
        get => _output ??= Console.Out;
        set => _output = value ?? throw new ArgumentNullException(nameof(value), "Use TextWriter.Null to disable the output");
    }

    /// <summary>
    /// Gets or sets the standard error used for printing error information like parse errors.
    /// </summary>
    /// <value>The default is set to <see cref="Console.Error"/>.</value>
    /// <remarks>
    /// For testing purposes, it can be set to a new instance of <see cref="StringWriter"/>.
    /// </remarks>
    public TextWriter Error
    {
        get => _error ??= Console.Error;
        set => _error = value ?? throw new ArgumentNullException(nameof(value), "Use TextWriter.Null to disable the output");
    }
}
