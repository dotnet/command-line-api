// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.Validation;
using System.CommandLine.Validation.Traits;

namespace System.CommandLine;

// TODO: Add support for terminating validator. This is needed at least for required because it would be annoying to get an error that you forgot to enter something, and also all the validation errors for the default value Probably other uses, so generalize to termintating.
public class ValidationSubsystem : CliSubsystem
{
    // The Type key is the DataTrait type
    private readonly Dictionary<Type, DataValidator> dataValidators = [];
    private readonly Dictionary<Type, CommandValidator> commandValidators = [];

    public ValidationSubsystem(IAnnotationProvider? annotationProvider = null) 
        : base(ValidationAnnotations.Prefix, SubsystemKind.Validation, annotationProvider)
    {
        IEnumerable<DataValidator> dataValidatorList = [
            new RangeValidator(),
            ];
        IEnumerable<CommandValidator> commandValidatorList = [
            new InclusiveGroupValidator(),
            ];

        dataValidators = dataValidatorList.ToDictionary(v => v.TraitType);
        commandValidators = commandValidatorList.ToDictionary(v => v.TraitType);
    }

    // TODO: Force validators to be of the specified trait type
    public void AddOrReplaceCommandValidator<TCommmandTrait>(CommandValidator validator)
        where TCommmandTrait : Trait
        => commandValidators[typeof(TCommmandTrait)] = validator;

    public void RemoveCommandValidator<TCommmandTrait>()
        where TCommmandTrait : Trait
        => commandValidators.Remove(typeof(TCommmandTrait));

    public void AddDataValidator<TDataTrait>(DataValidator<TDataTrait> validator)
        where TDataTrait : Trait
        => dataValidators[typeof(TDataTrait)] = validator;

    public void RemoveDataValidator<TDataTrait>()
        where TDataTrait : Trait
        => dataValidators.Remove(typeof(TDataTrait));

    public void ClearValidators(Type type)
    {
        commandValidators.Clear();
        dataValidators.Clear();
    }

    protected internal override bool GetIsActivated(ParseResult? parseResult)
    {
        // TODO: If we decide to throw on null parseResult, then this is correct. If we decide that is normal/ok, this should also do the check as an optimization
        return true;
    }

    protected internal override void Execute(PipelineResult pipelineResult)
    {
        if (pipelineResult.ParseResult is null)
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
        CommandValueResult commandResult = pipelineResult.ParseResult.CommandValueResult;
            var commandResults = GetResultAndParents(commandResult);
        // Not sure whether to do commands or values first
        ValidateCommands(commandResults, errors, commandValidators, validationContext);
        ValidateValues(commandResults, errors, dataValidators, validationContext);
        pipelineResult.AddErrors(errors);

        // TODO: Consider which of these local methods to make protected and possibly overridable
        static void ValidateValues(IEnumerable<CommandValueResult> commandResults, List<ParseError> errors,
            Dictionary<Type, DataValidator> validators, ValidationContext validationContext)
        {
            var dataSymbols = GetDataSymbols(commandResults);
            foreach (var dataSymbol in dataSymbols)
            {
                ValidateValue(dataSymbol, errors, validators, validationContext);
            }
        }

        static void ValidateValue(CliDataSymbol dataSymbol, List<ParseError> errors, Dictionary<Type, DataValidator> validators, ValidationContext validationContext)
        {
            // TODO: If this remains local, this test may not be needed
            if (validationContext.ParseResult is null)
            {
                // Nothing to do, validation is called prior to parsing. Any error should be reported elsewhere
                return;
            }
            var traits = dataSymbol.GetDataTraits();
            if (traits is null)
            {
                return; // This is a common case, and nothing to do
            }
            var value = validationContext.PipelineResult.GetValue(dataSymbol);
            var valueResult = validationContext.ParseResult.GetValueResult(dataSymbol);
            foreach (var trait in traits)
            {
                if (!validators.TryGetValue(trait.GetType(), out var validator))
                {
                    // TODO: This seems an issue - an exception or an error that a validator is missing
                    continue;
                }
                var newErrors = validator.Validate(value, valueResult, trait, validationContext);
                if (newErrors is not null)
                {
                    errors.AddRange(newErrors);
                }
            }
        }

        static IEnumerable<CliDataSymbol> GetDataSymbols(IEnumerable<CommandValueResult> commandResults)
            => commandResults
                .SelectMany(cr => cr.ValueResults
                .Select(c => c.ValueSymbol))
                .Distinct()
                .ToList();

        static IEnumerable<CommandValueResult> GetResultAndParents(CommandValueResult commandResult)
        {
            var list = new List<CommandValueResult>();
            var current = commandResult;
            while (current is not null)
            {
                list.Add(current);
                current = current.Parent;
            }
            return list;
        }

        static void ValidateCommands(IEnumerable<CommandValueResult> commandValueResults, List<ParseError> errors,
        Dictionary<Type, CommandValidator> validators, ValidationContext validationContext)
        {
            // Walk up the results tree. Not needed for ValueResults because they are collapsed
            foreach (var commandValueResult in commandValueResults)
            {
                var symbol = commandValueResult.Command;
                var traits = symbol.GetCommandTraits();
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
