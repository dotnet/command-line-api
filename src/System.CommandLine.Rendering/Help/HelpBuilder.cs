using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Rendering.Help
{
    public class HelpBuilder : IHelpBuilder
    {
        protected IConsole _console;
        private readonly ConsoleRenderer _renderer;

        /// <summary>
        /// Brokers the generation and output of help text of <see cref="Symbol"/>
        /// and the <see cref="IConsole"/>
        /// </summary>
        /// <param name="console"><see cref="IConsole"/> instance to write the help text output</param>
        /// <param name="columnGutter">
        /// Number of characters to pad invocation information from their descriptions
        /// </param>
        /// <param name="indentationSize">Number of characters to indent new lines</param>
        /// <param name="maxWidth">
        /// Maximum number of characters available for each line to write to the console
        /// </param>
        public HelpBuilder(
            IConsole console,
            ConsoleRenderer renderer)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            this._renderer = renderer;
        }

        /// <inheritdoc />
        public void Write(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Region region = _console is ITerminal terminal
                ? terminal.GetRegion()
                : new Region(0, 0, 2000, 1);

          (new SynopsisView(command)).Render(_renderer, region);

        }

        //private void AddSynopsis(ICommand command)
        //{
        //    if (!ShouldShowHelp(command))
        //    {
        //        return;
        //    }

        //    //var title = $"{command.Name}:";
        //    //HelpSection.Write(this, title, command.Description);

        //    var synopsis = new SynopsisView(command);
        //}

        //internal bool ShouldShowHelp(ISymbol symbol)
        //{
        //    if (symbol.IsHidden)
        //    {
        //        return false;
        //    }

        //    if (symbol is IArgument)
        //    {
        //        return !symbol.IsHidden;
        //    }
        //    else
        //    {
        //        return !symbol.IsHidden &&
        //               (!string.IsNullOrWhiteSpace(symbol.Description) ||
        //                symbol.Arguments().Any(ShouldShowHelp));
        //    }
        //}
    }
}
