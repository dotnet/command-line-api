// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using System.CommandLine.Invocation;

namespace System.CommandLine.CompletionSuggestions
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await SuggestionDispatcher.Parser
                                             .InvokeAsync(args);
        }
    }
}
