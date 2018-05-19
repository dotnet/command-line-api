// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace System.CommandLine
{
    public interface IValidationMessages
    {
        string NoArgumentsAllowed(string option);
        string CommandAcceptsOnlyOneArgument(string command, int argumentCount);
        string FileDoesNotExist(string filePath);
        string OptionAcceptsOnlyOneArgument(string option, int argumentCount);
        string RequiredArgumentMissingForCommand(string command);
        string RequiredArgumentMissingForOption(string option);
        string RequiredCommandWasNotProvided();
        string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues);
        string UnrecognizedCommandOrArgument(string arg);
        string UnrecognizedOption(string unrecognizedOption, IReadOnlyCollection<string> allowedValues);
        string ResponseFileNotFound(string filePath);
        string ErrorReadingResponseFile(string filePath, IOException e);
    }
}
