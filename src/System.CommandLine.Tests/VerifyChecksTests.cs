using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

public class VerifyChecksTests
{
    [Fact]
    public Task Run() =>
        VerifyChecks.Run();
}