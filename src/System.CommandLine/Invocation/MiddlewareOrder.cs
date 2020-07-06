// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public enum MiddlewareOrder
    {
        ExceptionHandler = -2000,
        Configuration = -1000,
        Default = default,
        ErrorReporting = 1000,
    }

    internal enum MiddlewareOrderInternal
    {
        Startup = -4000,
        ExceptionHandler = -3000,
        EnvironmentVariableDirective = -2600,
        ConfigureConsole = -2500,
        RegisterWithDotnetSuggest = -2400,
        DebugDirective = -2300,
        ParseDirective = -2200,
        SuggestDirective = -2000,
        TypoCorrection = -1900,
        VersionOption = -1200,
        HelpOption = -1100,
        ParseErrorReporting = 1000,
    }
}