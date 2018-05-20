using System;
using Xunit;
using System.CommandLine;
using System.Linq;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    public class SuggestionDispatcherTests
    {
        [Fact]
        public void Test1()
        {
            // -p 12 -e "C:\Program Files\dotnet\dotnet.exe" "dotnet add"
            //SuggestionDispatcher.Dispatch(new[] { "-p", "12", "-e", @"C:\Program Files\dotnet\dotnet.exe", "dotnet add" });

            string[] args = @"-p 12 - e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add""".Tokenize().ToArray();
            SuggestionDispatcher.Dispatch(args);

        }


        [Fact]
        public void GetCompletionSuggestions()
        {
            Assert.Throws<System.IO.FileNotFoundException>( () =>
                SuggestionDispatcher.GetCompletionSuggestions("Bogus file name", "")
            );
        }
    }
}
