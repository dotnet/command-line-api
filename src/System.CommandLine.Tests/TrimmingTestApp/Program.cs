using System.CommandLine;
using System.CommandLine.Invocation;

var fileOption = new Argument<FileInfo>().LegalFileNamesOnly();

var command = new RootCommand
{
    fileOption
};

command.SetHandler((FileInfo file, InvocationContext ctx) =>
{
    ctx.Console.Write($"The file you chose was: {file}");
}, fileOption);

command.Invoke(args);
