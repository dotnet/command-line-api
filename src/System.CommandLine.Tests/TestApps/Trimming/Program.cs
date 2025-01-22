using System.CommandLine;
using System.CommandLine.Invocation;

Argument<FileInfo> fileArgument = new ("file");
fileArgument.AcceptLegalFileNamesOnly();

RootCommand command = new ()
{
    fileArgument
};

command.SetAction(parseResult =>
{
    Console.Write($"The file you chose was: {parseResult.GetValue(fileArgument)}");
});

command.Parse(args).Invoke();
