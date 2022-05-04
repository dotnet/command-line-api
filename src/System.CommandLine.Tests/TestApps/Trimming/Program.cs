using System.CommandLine;
using System.CommandLine.Invocation;

var fileArgument = new Argument<FileInfo>().LegalFileNamesOnly();

var command = new RootCommand
{
    fileArgument
};

command.SetHandler(context =>
{
    context.Console.Write($"The file you chose was: {context.ParseResult.GetValueForArgument(fileArgument)}");
});

command.Invoke(args);
