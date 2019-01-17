using System.CommandLine.Invocation;

namespace System.CommandLine
{
    internal class HelpBuilderFactory : IHelpBuilderFactory
    {
        public IHelpBuilder CreateHelpBuilder(InvocationContext invocationContext)
        {
            return new HelpBuilder(invocationContext.Console);
        }
    }
}
