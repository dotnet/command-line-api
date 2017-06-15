// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine
{
    public interface IValidationMessages
    {
        string NoArgumentsAllowed(string option);
        string CommandAcceptsOnlyOneArgument(string command, int argumentCount);
        string FileDoesNotExist(string filePath);
        string CommandAcceptsOnlyOneSubcommand(string command, string subcommandsSpecified);
        string OptionAcceptsOnlyOneArgument(string option, int argumentCount);
        string RequiredArgumentMissingForCommand(string command);
        string RequiredArgumentMissingForOption(string option);
        string RequiredCommandWasNotProvided();
        string UnrecognizedArgument(string unrecognizedArg, string[] allowedValues);
        string UnrecognizedCommandOrArgument(string arg);
        string UnrecognizedOption(string unrecognizedOption, string[] allowedValues);
    }
}