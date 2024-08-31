// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

/// <summary>
/// Base class for validators that affect the entire command
/// </summary>
public abstract class CommandValidator : Validator
{
    protected CommandValidator(string name, Type valueConditionType, params Type[] moreValueConditionTypes)
        : base(name, valueConditionType, moreValueConditionTypes)
    {  }

    /// <summary>
    /// Validation method specific to command results.
    /// </summary>
    /// <param name="commandResult">The <see cref="CliCommandResult" that will be validated./></param>
    /// <param name="commandCondition">The <see cref="CommandCondition" that defines the condition that may be validatable./></param>
    /// <param name="validationContext">The <see cref="ValidationContext" containing information about the current context./></param>
    public abstract void Validate(CliCommandResult commandResult, CommandCondition commandCondition, ValidationContext validationContext);
}
