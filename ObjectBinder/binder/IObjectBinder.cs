using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public interface IObjectBinder
    {
        Command Command { get; }
        Task<int> Bind( InvocationContext context );
    }

    public interface IObjectBinder<out TModel> : IObjectBinder
        where TModel : IRootObjectModel
    {
        TModel Target { get; }
    }
}