using System.CommandLine.Invocation;
using System.IO;
using System.Threading;

namespace System.CommandLine;

public class InvocationConfiguration
{
    private TextWriter? _output, _error;

    /// <summary>
    /// Enables a default exception handler to catch any unhandled exceptions thrown during invocation. Enabled by default.
    /// </summary>
    public bool EnableDefaultExceptionHandler { get; set; } = true;

    /// <summary>
    /// Enables signaling and handling of process termination (Ctrl+C, SIGINT, SIGTERM) via a <see cref="CancellationToken"/> 
    /// that can be passed to a <see cref="CommandLineAction"/> during invocation.
    /// If not provided, a default timeout of 2 seconds is enforced.
    /// </summary>
    public TimeSpan? ProcessTerminationTimeout { get; set; }

    /// <summary>
    /// The standard output. Used by Help and other facilities that write non-error information.
    /// By default it's set to <see cref="Console.Out"/>.
    /// For testing purposes, it can be set to a new instance of <see cref="StringWriter"/>.
    /// If you want to disable the output, please set it to <see cref="TextWriter.Null"/>.
    /// </summary>
    public TextWriter Output
    {
        get => _output ??= Console.Out;
        set => _output = value ?? throw new ArgumentNullException(nameof(value), "Use TextWriter.Null to disable the output");
    }

    /// <summary>
    /// The standard error. Used for printing error information like parse errors.
    /// By default it's set to <see cref="Console.Error"/>.
    /// For testing purposes, it can be set to a new instance of <see cref="StringWriter"/>.
    /// </summary>
    public TextWriter Error
    {
        get => _error ??= Console.Error;
        set => _error = value ?? throw new ArgumentNullException(nameof(value), "Use TextWriter.Null to disable the output");
    }
}