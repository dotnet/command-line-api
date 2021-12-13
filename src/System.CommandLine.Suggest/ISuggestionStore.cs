// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace System.CommandLine.Suggest
{
    public interface ISuggestionStore
    {
        string GetCompletions(string exeFileName, string suggestionTargetArguments, TimeSpan timeout);
    }
}

