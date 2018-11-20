using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public interface IInvocationProvider
    {
        Func<T, Task<int>> InvokeAsyncFunc<T>();
    }
}
