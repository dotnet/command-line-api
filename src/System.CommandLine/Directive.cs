using System.Collections.Generic;
using System.CommandLine.Completions;

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
        /// <param name="description">The description of the directive, shown in help.</param>
        public Directive(string name, string? description)
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
        }

        private protected override string DefaultName => throw new NotImplementedException();

        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
            => Array.Empty<CompletionItem>();
    }
}
