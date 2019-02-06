using System.CommandLine.Binding;
using System.CommandLine.Invocation;

namespace System.CommandLine
{
    public interface IHelpBuilderFactory
    {
        IHelpBuilder CreateHelpBuilder(BindingContext context);
    }
}
