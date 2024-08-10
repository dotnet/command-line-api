// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// Thrown when an annotation value does not match the expected type for that <see cref="AnnotationId"/>.
/// </summary>
public class AnnotationTypeException(AnnotationId annotationId, Type expectedType, Type? actualType, IAnnotationProvider? provider = null) : Exception
{
    public AnnotationId AnnotationId { get; } = annotationId;
    public Type ExpectedType { get; } = expectedType;
    public Type? ActualType { get; } = actualType;
    public IAnnotationProvider? Provider = provider;

    public override string Message
    {
        get
        {
            if (Provider is not null)
            {
                return
                    $"Typed accessor for annotation '${AnnotationId}' expected type '{ExpectedType}' but the annotation provider returned an annotation of type '{ActualType?.ToString() ?? "[null]"}'. " +
                    $"This may be an authoring error in in the annotation provider '{Provider.GetType()}' or in a typed annotation accessor.";

            }

            return
                $"Typed accessor for annotation '${AnnotationId}' expected type '{ExpectedType}' but the stored annotation is of type '{ActualType?.ToString() ?? "[null]"}'. " +
                $"This may be an authoring error in a typed annotation accessor, or the annotation may have be stored directly with the incorrect type, bypassing the typed accessors.";
        }
    }
}