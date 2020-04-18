using System.CommandLine.Invocation;

namespace ObjectBinder
{
    public interface IModelWithHelp
    {
        bool HelpRequested { get; set; }
    }
}