// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.Validation;
using System.CommandLine.Validation.DataTraits;

namespace System.CommandLine;

public class ValidationSubsystem(IAnnotationProvider? annotationProvider = null)
    : CliSubsystem(ValidationAnnotations.Prefix, SubsystemKind.Validation, annotationProvider)
{
    // The Type key is the DataTrait type
    private readonly Dictionary<Type, DataValidator> dataValidators = [];
    private readonly Dictionary<Type, CommandValidator> commandValidators = [];

    public void AddValidator<TDataTrait>(CommandValidator validator)
        where TDataTrait : DataTrait
        => commandValidators[typeof(TDataTrait)] = validator;

    public void RemoveValidator<TDataTrait>(CommandValidator validator)
        where TDataTrait : DataTrait
        => commandValidators.Remove(typeof(TDataTrait));

    public void AddValidator<TDataTrait>(DataValidator validator)
        where TDataTrait : DataTrait
        => dataValidators[typeof(TDataTrait)] = validator;

    public void RemoveValidator<TDataTrait>(DataValidator validator)
        where TDataTrait : DataTrait
        => dataValidators.Remove(typeof(TDataTrait));

    public void ClearValidators(Type type)
    {
        commandValidators.Clear();
        dataValidators.Clear();
    }

    protected internal override void Execute(PipelineResult pipelineResult)
    {
        var validationContext = new ValidationContext(pipelineResult.Pipeline, this);
        var errors = new List<ParseError>();
        if (pipelineResult.ParseResult is null)
        {
            return; // nothing to do
        }
        // Not sure whether to do commands or values first
        ValidateCommands(pipelineResult.ParseResult.CommandValueResult, errors, commandValidators, validationContext);
        ValidateValues(pipelineResult.ParseResult.AllValueResults, errors, dataValidators,validationContext);

        static void ValidateValues(IEnumerable<ValueResult> allValueResults, List<ParseError> errors,
            Dictionary<Type, DataValidator> validators, ValidationContext validationContext)
        {
            foreach (var result in allValueResults)
            {
                if (result.ValueSymbol is not CliDataSymbol dataSymbol)
                {
                    return; // nothing to do
                }
                var traits = dataSymbol.GetDataTraits();
                if (traits is null)
                {
                    return;
                }
                foreach (var trait in traits)
                {
                    if (!validators.TryGetValue(trait.GetType(), out var validator))
                    {
                        // TODO: This seems an issue - an exception or an error that a validator is missing
                        continue;
                    }
                    var newErrors = validator.Validate(result, trait, validationContext);
                    if (newErrors is not null)
                    {
                        errors.AddRange(newErrors);
                    }
                }
            }
        }

        static void ValidateCommands(CommandValueResult commandValueResult, List<ParseError> errors,
            Dictionary<Type, CommandValidator> validators, ValidationContext validationContext)
        {
            // Walk up the results tree. Not needed for ValueResults because they are collapsed
            var current = commandValueResult;
            while (current is not null)
            {
                var symbol = current.Command;
                var traits = symbol.GetDataTraits();
                if (traits is null)
                {
                    return;
                }
                foreach (var trait in traits)
                {
                    if (!validators.TryGetValue(trait.GetType(), out var validator))
                    {
                        // TODO: This seems an issue - an exception or an error that a validator is missing
                        continue;
                    }
                    var newErrors = validator.Validate(commandValueResult, trait, validationContext);
                    if (newErrors is not null)
                    {
                        errors.AddRange(newErrors);
                    }
                }
                current = current.Parent;
            }
        }

        //static void GetValidationErrors(List<ParseError> errors, Dictionary<Type, Validator> validators,
        //    Pipeline pipeline, ValidationSubsystem validationSubsystem, CliSymbol symbol)
        //{
        //    var traits = symbol.GetDataTraits();
        //    if (traits is null)
        //    {
        //        return;
        //    }
        //    foreach (var trait in traits)
        //    {
        //        if (!validators.TryGetValue(trait.GetType(), out var validator))
        //        {
        //            // TODO: This seems an issue - an exception or an error that a validator is missing
        //            continue;
        //        }
        //        var newErrors = validator.Validate(symbol, trait, validationContext);
        //        if (newErrors is not null)
        //        {
        //            errors.AddRange(newErrors);
        //        }
        //    }
        //}
    }
}
