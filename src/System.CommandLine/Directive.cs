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
        /// <summary>
        /// Initializes a new instance of the Directive class.
        /// </summary>
        /// <param name="name">The name of the directive. It can't contain whitespaces.</param>
        /// <param name="syncHandler">The synchronous action that is invoked when directive is parsed.</param>
        /// <param name="asyncHandler">The asynchronous action that is invoked when directive is parsed.</param>
        public Directive(string name, 
            Action<InvocationContext>? syncHandler = null,
            Func<InvocationContext, CancellationToken, Task>? asyncHandler = null)
            : base(name)
        {
            if (syncHandler is not null)
            {
                SetSynchronousHandler(syncHandler);
            }
            else if (asyncHandler is not null)
            {
                SetAsynchronousHandler(asyncHandler);
            }
        }

        public void SetAsynchronousHandler(Func<InvocationContext, CancellationToken, Task> handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Handler = new AnonymousCommandHandler(handler);
        }

        public void SetSynchronousHandler(Action<InvocationContext> handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Handler = new AnonymousCommandHandler(handler);
        }

        internal ICommandHandler? Handler { get; private set; }

        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
            => Array.Empty<CompletionItem>();
    }
}
