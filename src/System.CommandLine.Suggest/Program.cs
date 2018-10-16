// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Suggest
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var provider = new CombineSuggestionRegistration(
                new GlobalToolsSuggestionRegistration(),
                new FileSuggestionRegistration());
            var dispatcher = new SuggestionDispatcher(provider);
            return await dispatcher.InvokeAsync(args);
        }
    }
}
