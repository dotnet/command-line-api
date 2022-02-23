// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal enum ArgumentConversionResultType
    {
        NoArgument, // NoArgumentConversionResult
        Successful, // SuccessfulArgumentConversionResult
        Failed, // FailedArgumentConversionResult
        FailedArity, // FailedArgumentConversionArityResult
        FailedType, // FailedArgumentTypeConversionResult
        FailedTooManyArguments, // TooManyArgumentsConversionResult
        FailedMissingArgument, // MissingArgumentConversionResult
    }
}