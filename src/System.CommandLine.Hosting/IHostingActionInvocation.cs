using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Hosting;

public interface IHostingActionInvocation
{
    Task<int> InvokeAsync(CancellationToken cancelToken = default);
}
