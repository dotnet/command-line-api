using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public interface IInvocationProvider
    {
        Func<Task<int>> InvokeAsyncFunc<T>(T command)
            where T:Command;
    }
}
