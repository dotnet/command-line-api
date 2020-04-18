using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

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

        public void Parse( CommandLineBuilder builder, string[] args )
        {
            ParseResult = builder.Build().Parse( args );

            HelpRequested = ParseResult.FindResultFor( builder.HelpOption ) != null;
        }

        public void Parse( CommandLineBuilder builder, string args )
        {
            ParseResult = builder.Build().Parse(args);

            HelpRequested = ParseResult.FindResultFor(builder.HelpOption) != null;
        }
    }
}