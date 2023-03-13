using BenchmarkDotNet.Attributes;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace System.CommandLine.Benchmarks.CommandLine
{
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Simple
    {
        [Params(new string[0], new string[4] { "--bool", "true", "-s", "test" })]
        public string[] Args { get; set; }

        [Benchmark]
        public int DefaultsSync() => BuildCommand().Invoke(Args);

        [Benchmark]
        public Task<int> DefaultsAsync() => BuildCommand().InvokeAsync(Args);

        [Benchmark]
        public int MinimalSync() => new CommandLineBuilder(BuildCommand()).Build().Invoke(Args);

        [Benchmark]
        public Task<int> MinimalAsync() => new CommandLineBuilder(BuildCommand()).Build().InvokeAsync(Args);

        private static RootCommand BuildCommand()
        {
            Option<bool> boolOption = new("--bool", "-b") { Description = "Bool option" };
            Option<string> stringOption = new("--string", "-s") { Description = "String option" };

            RootCommand command = new()
            {
                boolOption,
                stringOption
            };

            command.SetHandler(ctx => 
            {
                bool boolean = ctx.ParseResult.GetValue(boolOption);
                string text = ctx.ParseResult.GetValue(stringOption);
            });

            return command;
        }
    }
}
