using System.Collections.Generic;
using System.CommandLine.Completions;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Threading;

namespace System.CommandLine
{
    /// <summary>
    /// The purpose of directives is to provide cross-cutting functionality that can apply across command-line apps.
    /// Because directives are syntactically distinct from the app's own syntax, they can provide functionality that applies across apps.
    /// 
    /// A directive must conform to the following syntax rules:
    /// * It's a token on the command line that comes after the app's name but before any subcommands or options.
    /// * It's enclosed in square brackets.
    /// * It doesn't contain spaces.
    /// </summary>
    public class Directive : Symbol
    {
        internal Action<InvocationContext, ICommandHandler?>? SyncHandler;
        internal Func<InvocationContext, ICommandHandler?, CancellationToken, Task>? AsyncHandler;

        /// <summary>
        /// Initializes a new instance of the Directive class.
        /// </summary>
        /// <param name="name">The name of the directive. It can't contain whitespaces.</param>
        /// <param name="description">The description of the directive, shown in help.</param>
        /// <param name="syncHandler">The synchronous action that is invoked when directive is parsed.</param>
        /// <param name="asyncHandler">The asynchronous action that is invoked when directive is parsed.</param>
        /// <remarks>The second argument of both handlers is next handler than can be invoked.
        /// Example: a custom directive might just change current culture and run actual command afterwards.</remarks>
        public Directive(string name, 
            string? description = null, 
            Action<InvocationContext, ICommandHandler?>? syncHandler = null,
            Func<InvocationContext, ICommandHandler?, CancellationToken, Task>? asyncHandler = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null, empty, or consist entirely of whitespace.");
            }

            for (var i = 0; i < name.Length; i++)
            {
                if (char.IsWhiteSpace(name[i]))
                {
                    throw new ArgumentException($"Name cannot contain whitespace: \"{name}\"", nameof(name));
                }
            }

            Name = name;
            Description = description;

            SyncHandler = syncHandler;
            AsyncHandler = asyncHandler;
        }

        internal bool HasHandler => SyncHandler != null || AsyncHandler != null;

        public void SetAsynchronousHandler(Func<InvocationContext, ICommandHandler?, CancellationToken, Task> handler)
            => AsyncHandler = handler ?? throw new ArgumentNullException(nameof(handler));

        public void SetSynchronousHandler(Action<InvocationContext, ICommandHandler?> handler)
            => SyncHandler = handler ?? throw new ArgumentNullException(nameof(handler));

        private protected override string DefaultName => throw new NotImplementedException();

        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
            => Array.Empty<CompletionItem>();
    }
}
