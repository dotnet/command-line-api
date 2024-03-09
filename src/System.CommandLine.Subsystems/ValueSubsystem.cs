// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public class ValueSubsystem : CliSubsystem
{
    private ParseResult? parseResult = null;

    public ValueSubsystem(IAnnotationProvider? annotationProvider = null)
        : base(ValueAnnotations.Prefix, SubsystemKind.Version, annotationProvider)
    {    }

    void SetExplicit(CliSymbol symbol, object value)
        => SetAnnotation(symbol, ValueAnnotations.Explicit, value);
    object GetExplicit(CliSymbol symbol)
      => TryGetAnnotation(symbol, ValueAnnotations.Explicit, out var value)
                ? value
                : "";
    AnnotationAccessor<object> Explicit
      => new(this, ValueAnnotations.Explicit);

    void SetCalculated(CliSymbol symbol, Func<ValueResult, object?> factory)
        => SetAnnotation(symbol, ValueAnnotations.Calculated, factory);
    Func<ValueResult, object?> GetCalculated(CliSymbol symbol)
      => TryGetAnnotation<Func<ValueResult, object?>>(symbol, ValueAnnotations.Calculated, out var value)
                ? value
                : null;
    AnnotationAccessor<Func<ValueResult, object?>> Calculated
      => new(this, ValueAnnotations.Calculated);

    protected internal override bool GetIsActivated(ParseResult? parseResult)
    {
        this.parseResult = parseResult;
        return true;
    }
}

