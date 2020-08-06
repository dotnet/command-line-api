using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Localization;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace LocalizationPlayground
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var parser = new CommandLineBuilder(
                new RootCommand
                {
                    Description = "Playground for localized CommandLine",
                    Handler = CommandHandler.Create((int count, string name, InvocationContext invocation, IStringLocalizerFactory localizerFactory) =>
                    {
                        var cult = CultureInfo.CurrentUICulture;
                        var germanResourceNames = typeof(Program).Assembly
                            .GetSatelliteAssembly(CultureInfo.GetCultureInfo("de"))
                            .GetManifestResourceNames();

                        var localizer = localizerFactory.Create(typeof(Program));
                        var locCultureInfo = localizer.GetString("Current culture: {0}", cult.NativeName);
                        Console.WriteLine(locCultureInfo);
                        var locLine = localizer.GetString("Hello {0}!", name);

                        var availableStrings = localizer.GetAllStrings(true);

                        _ = germanResourceNames;
                        _ = availableStrings;

                        for (int i = 0; i < count; i++)
                        {
                            Console.WriteLine(locLine);
                        }

                        Console.WriteLine();
                        invocation.InvocationResult = new HelpResult();
                    }),
                })
                .AddOption(new Option<int>(new[] { "--count", "-c" }, () => 1)
                {
                    Name = "count",
                    Description = "Count of lines to print",
                    Argument =
                    {
                        Name = "COUNT",
                        Description = "An integer value",
                        Arity = ArgumentArity.ZeroOrOne,
                    }
                })
                .AddArgument(new Argument<string>("NAME")
                {
                    Description = "The name to display",
                    Arity = ArgumentArity.ExactlyOne,
                })
                .UseEnvironmentVariableDirective()
                .UseDebugDirective()
                .UseHelp()
                .UseCultureEnvironment()
                .UseLocalization()
                .Build();
            return parser.InvokeAsync(args ?? Array.Empty<string>());
        }
    }
}
