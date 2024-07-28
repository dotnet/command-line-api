// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.Validation;

namespace System.CommandLine;

public class ValidationSubsystem(IAnnotationProvider? annotationProvider = null)
    : CliSubsystem(ValidationAnnotations.Prefix, SubsystemKind.Validation, annotationProvider)
{
    private readonly Dictionary<Type, List<Validator>> validators = [];

    public void AddValidator(Type type, Validator validator)
    {
        if (!validators.TryGetValue(type, out var validatorList))
        {
            validators.Add(type, [validator]);
            return;
        }
        validatorList.Add(validator);
    }

    public void RemoveValidator(Type type, Validator validator)
    {
        if (!validators.TryGetValue(type, out var validatorList))
        {
            return; // nothing to do
        }
        validatorList.Remove(validator);
    }

    public void ClearValidatorsForType(Type type)
    {
        if (!validators.TryGetValue(type, out var validatorList))
        {
            return; // nothing to do
        }
        validatorList.Clear();
    }

    public void ClearAllValidators(Type type)
    {
        validators.Clear();
    }

    protected internal override void Execute(PipelineResult pipelineResult)
    {
       // var symbolResults = pipelineResult.ParseResult.SymbolResults();
    }
}
