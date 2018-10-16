using System;

namespace System.CommandLine.StarFruit
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionAttribute : Attribute
    {
        public readonly string Description;
        public readonly string[] Aliases;
        public string Name { get; set; }

        public OptionAttribute(string description, params string[] aliases)
        {
            Description = description;
            Aliases = aliases;
        }
    }

    // KAD: Much more work here
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionWithArgumentAttribute : OptionAttribute
    {
        public readonly Arity Arity;

        public OptionWithArgumentAttribute(string description, Arity arity, params string[] aliases)
            : base (description , aliases )
        {
            Arity = arity;
        }
    }
}
