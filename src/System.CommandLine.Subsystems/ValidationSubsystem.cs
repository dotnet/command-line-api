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
    private readonly Dictionary<Type, Validator> validators = [];

    public void AddValidator<TDataTrait>(Validator validator)
        where TDataTrait : DataTrait
        => validators[typeof(TDataTrait)] = validator;

    public void RemoveValidator<TDataTrait>(Validator validator)
        where TDataTrait : DataTrait
        => validators.Remove(typeof(TDataTrait));

    public void ClearValidators(Type type)
        => validators.Clear();

    protected internal override void Execute(PipelineResult pipelineResult)
    {
        var errors = new List<ParseError>();
        if (pipelineResult.ParseResult is null)
        {
            return; // nothing to do
        }
        // Not sure whether to do commands or values first
        ValidateCommands(pipelineResult.ParseResult.CommandValueResult, errors, validators, pipelineResult.Pipeline, this);
        ValidateValues(pipelineResult.ParseResult.AllValueResults, errors, validators, pipelineResult.Pipeline, this);

        static void ValidateValues(IEnumerable<ValueResult> allValueResults, List<ParseError> errors,
            Dictionary<Type, Validator> validators, Pipeline pipeline, ValidationSubsystem validationSubsystem)
        {
            foreach (var result in allValueResults)
            {
                if (result.ValueSymbol is not CliDataSymbol dataSymbol)
                {
                    return; // nothing to do
                }
                GetValidationErrors(errors, validators, pipeline, validationSubsystem, dataSymbol);
            }
        }

        static void ValidateCommands(CommandValueResult commandValueResult, List<ParseError> errors,
            Dictionary<Type, Validator> validators, Pipeline pipeline, ValidationSubsystem validationSubsystem)
        {
            // Walk up the results tree. Not needed for ValueResults because they are collapsed
            var current = commandValueResult;
            while (current is not null)
            {
                var symbol = current.Command;
                GetValidationErrors(errors, validators, pipeline, validationSubsystem, symbol);
                current = current.Parent;
            }
        }

        static void GetValidationErrors(List<ParseError> errors, Dictionary<Type, Validator> validators,
            Pipeline pipeline, ValidationSubsystem validationSubsystem, CliSymbol symbol)
        {
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
                var newErrors = validator.Validate(symbol, trait, pipeline, validationSubsystem);
                if (newErrors is not null)
                {
                    errors.AddRange(newErrors);
                }
            }
        }
    }
}
