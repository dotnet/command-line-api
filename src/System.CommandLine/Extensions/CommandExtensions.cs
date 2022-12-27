namespace System.CommandLine.Extensions;

public static class CommandExtensions
{

    public static Option<T> AddOption<T>(
        this Command command,
        string name,
        string description
    )
    {
        var option = new Option<T>(
            name: name,
            description: description
        );
        command.Options.Add(option);
        return option;
    }

    public static Option<T> AddOption<T>(
        this Command command,
        string name,
        string description,
        Func<T>? defaultValueFactory = null
    )
    {
        var option = new Option<T>(
            name: name,
            description: description,
            defaultValueFactory: defaultValueFactory!
        );
        command.Options.Add(option);
        return option;
    }

}