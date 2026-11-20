using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace System.CommandLine.Benchmarks.CommandLine
{
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Simple
    {
        [Params(new string[0], new string[4] { "--bool", "true", "-s", "test" })]
        public string[] Args { get; set; }

        [Benchmark]
        public int DefaultsSync() => BuildCommand().Parse(Args).Invoke();

        [Benchmark]
        public Task<int> DefaultsAsync() => BuildCommand().Parse(Args).InvokeAsync();

        [Benchmark]
        public int MinimalSync() => BuildMinimalCommand().Parse(Args).Invoke(BuildMinimalConfig());

        [Benchmark]
        public Task<int> MinimalAsync() => BuildMinimalCommand().Parse(Args).InvokeAsync(BuildMinimalConfig());

        private static RootCommand BuildCommand()
        {
            Option<bool> boolOption = new("--bool", "-b") { Description = "Bool option" };
            Option<string> stringOption = new("--string", "-s") { Description = "String option" };

            RootCommand command = new()
            {
                boolOption,
                stringOption
            };

            command.SetAction(parseResult => 
            {
                bool boolean = parseResult.GetValue(boolOption);
                string text = parseResult.GetValue(stringOption);
            });

            return command;
        }

        private static RootCommand BuildMinimalCommand()
        {
            RootCommand command = BuildCommand();
            command.Directives.Clear();
            return command;
        }

        private static InvocationConfiguration BuildMinimalConfig() => new()
        {
            EnableDefaultExceptionHandler = false,
            ProcessTerminationTimeout = null
        };
    }
}
