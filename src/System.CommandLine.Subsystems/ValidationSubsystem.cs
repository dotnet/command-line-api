// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;
using System.CommandLine.Validation;

namespace System.CommandLine;

// TODO: Add support for terminating validator. This is needed at least for required because it would be annoying to get an error that you forgot to enter something, and also all the validation errors for the default value Probably other uses, so generalize to termintating.
public sealed class ValidationSubsystem : CliSubsystem
{
    // The type here is the ValueCondition type
    private Dictionary<Type, Validator> validators = [];

    private ValidationSubsystem(IAnnotationProvider? annotationProvider = null)
        : base("", SubsystemKind.Validation, annotationProvider)
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

    public Validator this[Type type]
    {
        get { return validators[type]; }
    }

    public void AddValidator(Validator validator)
    {
        foreach (var type in validator.ValueConditionTypes)
        {
            validators[type] = validator;
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
        var valueConditions = valueSymbol.GetValueConditions();
        if (valueConditions is null)
        {
            return; // nothing to do
        }

        var value = validationContext.PipelineResult.GetValue(valueSymbol);
        var valueResult = validationContext.ParseResult?.GetValueResult(valueSymbol);
        foreach (var condition in valueConditions)
        {
            ValidateValueCondition(value, valueSymbol, valueResult, condition, validationContext);
        }
    }

    private void ValidateCommand(CliCommandResult commandResult, ValidationContext validationContext)
    {
        var valueConditions = commandResult.Command.GetValueConditions();
        if (valueConditions is null)
        {
            return; // nothing to do
        }

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
            ? [commandResult, .. global::System.CommandLine.ValidationSubsystem.CommandAndAncestors(commandResult.Parent)]
            : [commandResult];

    private void ValidateValueCondition(object? value, CliValueSymbol valueSymbol, CliValueResult? valueResult, ValueCondition condition, ValidationContext validationContext)
    {
        Validator? validator = GetValidator(condition);
        switch (validator)
        {
            case null:
                break; 
            case ValueValidator valueValidator:
                valueValidator.Validate(value, valueSymbol, valueResult, condition, validationContext); 
                break;
            default:
                throw new InvalidOperationException("Validator must be derive from ValueValidator");
        }
    }

    private Validator? GetValidator(ValueCondition condition)
    {
        if (!validators.TryGetValue(condition.GetType(), out var validator) || validator is null)
        {
            if (condition.MustHaveValidator)
            {
                // Output missing validator error
            }
        }

        return validator;
    }

    private void ValidateCommandCondition(CliCommandResult commandResult, ValueCondition condition, ValidationContext validationContext)
    {
        Validator? validator = GetValidator(condition);
        switch (validator)
        {
            case null:
                break;
            case CommandValidator commandValidator:
                commandValidator.Validate(commandResult, condition, validationContext);
                break;
            default:
                throw new InvalidOperationException("Validator must be derive from CommandValidator");
        }
    }



    /*   if (pipelineResult.ParseResult is null)
        {
            // Nothing to do, validation is called prior to parsing. Is this an exception or error?
            return;
        }
        var validationContext = new ValidationContext(pipelineResult, this);
        var errors = new List<ParseError>();
        if (pipelineResult.ParseResult is null)
        {
            return; // nothing to do
        }
        CliCommandResult commandResult = pipelineResult.ParseResult.CommandResult;
        var commandResults = GetResultAndParents(commandResult);
        // Not sure whether to do commands or values first
        ValidateCommands(commandResults, errors, commandValidators, validationContext);
        ValidateValues(commandResults, errors, dataValidators, validationContext);
        pipelineResult.AddErrors(errors);

        // TODO: Consider which of these local methods to make protected and possibly overridable
        static void ValidateValues(IEnumerable<CliCommandResult> commandResults, List<ParseError> errors,
            Dictionary<Type, DataValidator> validators, ValidationContext validationContext)
        {
            var dataSymbols = GetDataSymbols(commandResults);
            foreach (var dataSymbol in dataSymbols)
            {
                ValidateValue(dataSymbol, errors, validators, validationContext);
            }
        }

        static void ValidateValue(CliValueSymbol dataSymbol, List<ParseError> errors, Dictionary<Type, DataValidator> validators, ValidationContext validationContext)
        {
            // TODO: If this remains local, this test may not be needed
            if (validationContext.ParseResult is null)
            {
                // Nothing to do, validation is called prior to parsing. Any error should be reported elsewhere
                return;
            }
            var valueConditions = dataSymbol.GetValueConditions();
            if (valueConditions is null)
            {
                return; // This is a common case, and nothing to do
            }
            var value = validationContext.PipelineResult.GetValue(dataSymbol);
            var valueResult = validationContext.ParseResult.GetValueResult(dataSymbol);
            foreach (var valueCondition in valueConditions)
            {
                if (!validators.TryGetValue(valueCondition.GetType(), out var validator))
                {
                    // TODO: This seems an issue - an exception or an error that a validator is missing
                    continue;
                }
                var newErrors = validator.Validate(value, valueResult, valueCondition, validationContext);
                if (newErrors is not null)
                {
                    errors.AddRange(newErrors);
                }
            }
        }

        static IEnumerable<CliValueSymbol> GetDataSymbols(IEnumerable<CliCommandResult> commandResults)
            => commandResults
                .SelectMany(cr => cr.ValueResults
                .Select(c => c.ValueSymbol))
                .Distinct()
                .ToList();

        static IEnumerable<CliCommandResult> GetResultAndParents(CliCommandResult commandResult)
        {
            var list = new List<CliCommandResult>();
            var current = commandResult;
            while (current is not null)
            {
                list.Add(current);
                current = current.Parent;
            }
            return list;
        }

        static void ValidateCommands(IEnumerable<CliCommandResult> commandValueResults, List<ParseError> errors,
        Dictionary<Type, CommandValidator> validators, ValidationContext validationContext)
        {
            // Walk up the results tree. Not needed for ValueResults because they are collapsed
            foreach (var commandValueResult in commandValueResults)
            {
                var symbol = commandValueResult.Command;
                var valueConditions = symbol.GetCommandValueConditions();
                if (valueConditions is null)
                {
                    return;
                }
                foreach (var valueCondition in valueConditions)
                {
                    if (!validators.TryGetValue(valueCondition.GetType(), out var validator))
                    {
                        // TODO: This seems an issue - an exception or an error that a validator is missing
                        continue;
                    }
                    var newErrors = validator.Validate(commandValueResult, valueCondition, validationContext);
                    if (newErrors is not null)
                    {
                        errors.AddRange(newErrors);
                    }
                }
            }
        }
    }
    */
}
