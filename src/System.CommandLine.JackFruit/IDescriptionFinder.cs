namespace System.CommandLine.JackFruit
{
    public interface IDescriptionFinder
    {
        string Description<TSource>(TSource source);
    }
}
