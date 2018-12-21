namespace System.CommandLine.DragonFruit
{
    public class ParameterMetadata
    {
        public ParameterMetadata(string name, string description, string alias = null)
        {
            Name = name;
            Description = description;
            Alias = alias;
        }

        public string Name { get; }

        public string Description { get; }

        public string Alias { get; }
    }
}