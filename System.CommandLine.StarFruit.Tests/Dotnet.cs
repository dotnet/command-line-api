namespace System.CommandLine.StarFruit.Tests
{
    [Command("Run dotnet commands.")]
    public class Dotnet :Command 
    {
        [Command("Initialize .NET projects.")]
        public class New : Command
        {
            [Option("Lists templates containing the specified name.If no name is specified, lists all templates.", "l")]
            public bool List { get; }

            [OptionWithArgument("The name for the output being created. If no name is specified, the name of the current directory is used.", Arity.ExactlyOne, "n")]
            public string Name { get; }

            [OptionWithArgument("Location to place the generated output.", Arity.ExactlyOne ,"o")]
            public string Output { get; }

            [OptionWithArgument("Installs a source or a template pack", Arity.ExactlyOne, "i")]
            public string Install { get; }

            [OptionWithArgument("Uninstalls a source or a template pack.", Arity.ExactlyOne, "u")]
            public string Uninstall { get; }

            [OptionWithArgument("Specifies a NuGet source to use during install.",Arity.ExactlyOne)]
            public string NugetSource { get; }

            [OptionWithArgument(@"Filters templates based on available types.Predefined values are ""project"", ""item"" or ""other"".", Arity.ExactlyOne)]
            public TemplateType Type { get; }

            [Option("Forces content to be generated even if it would change existing files.")]
            public bool Force { get; }

            [OptionWithArgument("Filters templates based on language and specifies the language of the template to create.", Arity.ExactlyOne, "lang")]
            public ProgrammingLanguage Language { get; }
        }

        [Command("Modify solution (SLN) files.")]
        public class Sln : Command
        {

            [CommandArgument( "Solution file to operate on.If not specified, the command will search the current directory for one.", Arity.ZeroOrOne)]
            public Argument SlnFile { get; }

            [Command("Add project(s) to a solution file.")]
            public class Add
            { }

            [Command("List project(s) in a solution file.")]
            public class List
            { }

            [Command("Remove project(s) from a solution file.")]
            public class Remove
            { }

        }

        public enum ProgrammingLanguage
        {
            Unknown = 0,
            CSharp,
            VisualBasic,
            FSharp
        }

        public enum TemplateType
        {
            Unknown = 0,
            Project,
            Item,
            Other
        }
    }


}
