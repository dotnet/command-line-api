using System.CommandLine.Validation.Traits;

namespace System.CommandLine.Subsystems.Annotations;

public class GeneralAnnotations
{
    public static string Prefix { get; } = string.Empty;

    public static AnnotationId<List<Trait>> DataTraits { get; } = new(Prefix, nameof(DataTraits));
}
