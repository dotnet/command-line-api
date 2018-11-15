namespace System.CommandLine.JackFruit
{
    public interface IHelpProvider
    {
        string Help<T>(T command, string Name)
            where T : Command;
    }
}
