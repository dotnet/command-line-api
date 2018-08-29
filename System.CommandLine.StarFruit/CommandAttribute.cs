namespace System.CommandLine.StarFruit
{
    public class Command
    {
        public Command SubCommand { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class )]
    public class CommandAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Description;


        public CommandAttribute(string description, string name = null)
        {
            Name = name;
            Description = description;
        }
    }
}
