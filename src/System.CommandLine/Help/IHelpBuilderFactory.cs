using System.CommandLine.Invocation;

namespace System.CommandLine
{
    public interface IHelpBuilderFactory
    {
        IHelpBuilder CreateHelpBuilder(InvocationContext invocationContext);
    }
}
