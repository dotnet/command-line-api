namespace System.CommandLine.JackFruit
{
    public interface IDescriptionProvider<TSource>
    {
        string Description(TSource source);
        string Description(TSource source, string Name);
    }
}
