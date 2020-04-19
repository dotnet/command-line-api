using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace ObjectBinder
{
    public class ObjectBindingModel : IObjectBindingModel
    {
        protected ObjectBindingModel()
        {
        }

        public ParseResult ParseResult { get; private set; }
        public bool HelpRequested { get; private set; }

        public void Parse( IObjectBinder objBinder, string[] args, IConsole console = null )
        {
            var builder = new CommandLineBuilder(objBinder.Command)
                .UseDefaults()
                .UseObjectBinder(objBinder);

            Parse( builder, args, console  );
        }

        public void Parse( CommandLineBuilder builder, string[] args, IConsole console = null )
        {
            ParseResult = builder.Build().Parse( args );
            HelpRequested = ParseResult.FindResultFor(builder.HelpOption) != null;

            ParseResult.Invoke( console );
        }

        public void Parse(IObjectBinder objBinder, string args, IConsole console = null)
        {
            var builder = new CommandLineBuilder(objBinder.Command)
                .UseDefaults()
                .UseObjectBinder(objBinder);

            Parse(builder, args, console);
        }

        public void Parse( CommandLineBuilder builder, string args, IConsole console = null )
        {
            ParseResult = builder.Build().Parse( args );
            HelpRequested = ParseResult.FindResultFor( builder.HelpOption ) != null;

            ParseResult.Invoke( console );
        }
    }
}