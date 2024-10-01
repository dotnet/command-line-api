// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;
using System.CommandLine.Validation;

namespace System.CommandLine;

// TODO: Add support for terminating validator. This is needed at least for required because it would be annoying to get an error that you forgot to enter something, and also all the validation errors for the default value Probably other uses, so generalize to termintating.
public sealed class ValidationSubsystem : CliSubsystem
{
    // The type here is the ValueCondition type
    private Dictionary<Type, ValueValidator> valueValidators = [];
    private Dictionary<Type, CommandValidator> commandValidators = [];

    private ValidationSubsystem()
        : base("", SubsystemKind.Validation)
    { }

    public static ValidationSubsystem Create()
    {
        var newValidationSubsystem = new ValidationSubsystem();
        newValidationSubsystem.AddValidator(new RangeValidator());
        newValidationSubsystem.AddValidator(new InclusiveGroupValidator());
        return newValidationSubsystem;
    }

    public static ValidationSubsystem CreateEmpty()
        => new ValidationSubsystem();

    public void AddValidator(ValueValidator validator)
    {
        foreach (var type in validator.ValueConditionTypes)
        {
            valueValidators[type] = validator;
        }
    }

    public void AddValidator(CommandValidator validator)
    {
        foreach (var type in validator.ValueConditionTypes)
        {
            commandValidators[type] = validator;
        }
    }

    protected internal override bool GetIsActivated(ParseResult? parseResult) => true;

    public override void Execute(PipelineResult pipelineResult)
    {
        if (pipelineResult.ParseResult is null)
        {
            return;
        }
        var validationContext = new ValidationContext(pipelineResult, this);
        var commandResults = CommandAndAncestors(pipelineResult.ParseResult.CommandResult);
        var valueSymbols = GetValueSymbols(commandResults);
        foreach (var symbol in valueSymbols)
        {
            ValidateValue(symbol, validationContext);
        }
        foreach (var commandResult in commandResults)
        {
            ValidateCommand(commandResult, validationContext);
        }
    }

    private void ValidateValue(CliValueSymbol valueSymbol, ValidationContext validationContext)
    {
        var valueConditions = valueSymbol.EnumerateValueConditions();

        // manually implement the foreach so we can efficiently skip getting the
        // value if there are no conditions
        var enumerator = valueConditions.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return;
        }

        var value = validationContext.GetValue(valueSymbol);
        var valueResult = validationContext.GetValueResult(valueSymbol);

        do {
            ValidateValueCondition(value, valueSymbol, valueResult, enumerator.Current, validationContext);
        } while (enumerator.MoveNext());
    }

    private void ValidateCommand(CliCommandResult commandResult, ValidationContext validationContext)
    {
        var valueConditions = commandResult.Command.EnumerateCommandConditions();

        // unlike ValidateValue we do not need to manually implement the foreach
        // to skip unnecessary work, as there is no additional work to be before
        // calling command validators
        foreach (var condition in valueConditions)
        {
            ValidateCommandCondition(commandResult, condition, validationContext);
        }
    }

    private static List<CliValueSymbol> GetValueSymbols(IEnumerable<CliCommandResult> commandResults)
      => commandResults
        .SelectMany(commandResult => commandResult.ValueResults.Select(valueResult => valueResult.ValueSymbol))
        .Distinct()
        .ToList();

    // Consider moving to CliCommandResult
    private static IEnumerable<CliCommandResult> CommandAndAncestors(CliCommandResult commandResult)
        => commandResult.Parent is not null
            ? [commandResult, .. CommandAndAncestors(commandResult.Parent)]
            : [commandResult];

    private void ValidateValueCondition(object? value, CliValueSymbol valueSymbol, CliValueResult? valueResult, ValueCondition condition, ValidationContext validationContext)
    {
        if (condition is IValueValidator conditionValidator)
        {
            conditionValidator.Validate(value, valueSymbol, valueResult, condition, validationContext);
            return;
        }
        ValueValidator? validator = GetValidator(condition);
        if (validator == null)
        {
            if (condition.MustHaveValidator)
            {
                validationContext.AddError(new ParseError($"{valueSymbol.Name} must have {condition.Name} validator."));
            }
            return; 
        }
        validator.Validate(value, valueSymbol, valueResult, condition, validationContext);

    }

    private ValueValidator? GetValidator(ValueCondition condition)
    {
        if (!valueValidators.TryGetValue(condition.GetType(), out var validator) || validator is null)
        {
            if (condition.MustHaveValidator)
            {
                // Output missing validator error
            }
        }

        return validator;
    }

    private CommandValidator? GetValidator(CommandCondition condition)
    {
        if (!commandValidators.TryGetValue(condition.GetType(), out var validator) || validator is null)
        {
            if (condition.MustHaveValidator)
            {
                // Output missing validator error
            }
        }

        return validator;
    }

    private void ValidateCommandCondition(CliCommandResult commandResult, CommandCondition condition, ValidationContext validationContext)
    {
        if (condition is ICommandValidator conditionValidator)
        {
            conditionValidator.Validate(commandResult, condition, validationContext);
            return;
        }
        CommandValidator? validator = GetValidator(condition);
        if (validator == null)
        {
            if (condition.MustHaveValidator)
            {
                validationContext.AddError(new ParseError($"{commandResult.Command.Name} must have {condition.Name} validator."));
            }
            return;
        }
        validator.Validate(commandResult, condition, validationContext);
    }
}
