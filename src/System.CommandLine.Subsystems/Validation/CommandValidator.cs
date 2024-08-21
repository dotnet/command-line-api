// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

public abstract class CommandValidator : Validator
{
    protected CommandValidator(string name, Type valueConditionType, params Type[] moreValueConditionTypes)
        : base(name, valueConditionType, moreValueConditionTypes)
    {  }

    public abstract void Validate(CliCommandResult commandResult, CommandCondition commandCondition, ValidationContext validationContext);
}
