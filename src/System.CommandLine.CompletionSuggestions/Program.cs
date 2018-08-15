// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.CommandLine.Invocation;

namespace System.CommandLine.CompletionSuggestions
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(SuggestionDispatcher.Dispatch(args,
                new SuggestionFileProvider(), 20000));
        }
    }
}
