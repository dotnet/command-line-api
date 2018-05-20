using System;
using Xunit;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    public class SuggestionDispatcherTests
    {
        [Fact]
        public void Test1()
        {
            // -p 12 -e "C:\Program Files\dotnet\dotnet.exe" "dotnet add"
            SuggestionDispatcher.Dispatch(new[] { "-p", "12", "-e", @"""C:\Program Files\dotnet\dotnet.exe""", "\"dotnet add\"" });
        }
    }
}
