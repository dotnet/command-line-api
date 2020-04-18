using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace ObjectBinder
{
    public interface IObjectBindingModel
    {
        ParseResult ParseResult { get; }
        bool HelpRequested { get; }
    }

    public class ObjectBindingModel : IObjectBindingModel
    {
        protected ObjectBindingModel()
        {
        }

        public ParseResult ParseResult { get; private set; }
        public bool HelpRequested { get; private set; }

        public void Parse( CommandLineBuilder builder, string[] args, IObjectBinder objBinder, IConsole console = null )
        {
            ParseResult = builder.Build().Parse( args );

            CheckHelpShortCircuit(builder,  objBinder, console);
        }

        public void Parse( CommandLineBuilder builder, string args, IObjectBinder objBinder, IConsole console = null )
        {
            ParseResult = builder.Build().Parse(args);

            CheckHelpShortCircuit(builder, objBinder, console);
        }

        private void CheckHelpShortCircuit( CommandLineBuilder builder, IObjectBinder objBinder, IConsole console )
        {
            if (ParseResult.FindResultFor(builder.HelpOption) != null)
            {
                HelpRequested = true;

                var helpBuilder = new HelpBuilder( console ?? new SystemConsole() );

                helpBuilder.Write( objBinder.Command );
            }
        }
    }
}