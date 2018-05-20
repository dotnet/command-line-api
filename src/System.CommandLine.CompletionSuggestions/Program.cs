using System;

namespace System.CommandLine.CompletionSuggestions
{
    class Program
    {
        static void Main(string[] args)
        {
            SuggestionDispatcher.Dispatch(args);
        }
    }
}
