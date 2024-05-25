// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public class ValidationSubsystem(IAnnotationProvider? annotationProvider = null)
    : CliSubsystem(ValidationAnnotations.Prefix, SubsystemKind.Validation, annotationProvider)
{
    private readonly Dictionary<Type, List<(Type validationDataType, Func<object, object, bool> isValid)>> validators = [];

    //protected internal override void Execute(PipelineResult pipelineResult)
    //{
    //    var values = pipelineResult.Pipeline.Value
    //    if (!validators.TryGetValue()
    //}
}
