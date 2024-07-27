using System.CommandLine.Subsystems.DataTraits;

namespace System.CommandLine.Subsystems.Annotations;

public class GeneralAnnotations
{
    public static string Prefix { get; } = string.Empty;

    public static AnnotationId<List<DataTrait>> DataTraits { get; } = new(Prefix, nameof(DataTraits));
}
