using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace ObjectBinder
{
    public interface IObjectBinder
    {
        Command Command { get; }
        Task<int> Bind( InvocationContext context );
    }

    public interface IObjectBinder<out TModel> : IObjectBinder
    {
        TModel Target { get; }
    }
}