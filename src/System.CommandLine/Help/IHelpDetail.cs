namespace System.CommandLine
{
    public interface IHelpDetail
    {
        string Description { get; }

        bool IsHidden { get; }

        string Name { get; }
    }
}