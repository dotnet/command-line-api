namespace System.CommandLine.StarFruit
{
    public class Argument
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ArgumentAttribute : Attribute
    {
        public readonly Arity Arity;
        public readonly string Name;
        public readonly string Description;

        protected ArgumentAttribute(string description, Arity arity, string name)
        {
            Arity = arity;
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CommandArgumentAttribute : ArgumentAttribute
    {
        public CommandArgumentAttribute(string description, Arity arity, string name=null) : base(description, arity,name)
        { }
    }
}
