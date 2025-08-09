using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace System.CommandLine.Benchmarks.CommandLine
{
    [BenchmarkCategory(Categories.CommandLine)]
    public class Perf_Parser_Simple
    {
        [Params(new string[0], new string[4] { "--bool", "true", "-s", "test" })]
        public string[] Args { get; set; }

        private InvocationConfiguration _minimalInvocationConfig = new()
        {
            EnableDefaultExceptionHandler = false,
            ProcessTerminationTimeout = null
        };

        [Benchmark]
        public int DefaultsSync() => BuildCommand().Parse(Args).Invoke();

        [Benchmark]
        public Task<int> DefaultsAsync() => BuildCommand().Parse(Args).InvokeAsync();

        [Benchmark]
        public int MinimalSync() => BuildCommand(minimal: true).Parse(Args).Invoke();

        [Benchmark]
        public Task<int> MinimalAsync() => BuildCommand(minimal: true).Parse(Args).InvokeAsync(_minimalInvocationConfig);

        private static RootCommand BuildCommand(bool minimal = true)
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

            if (minimal)
            {
                command.Directives.Clear();
            }

            return command;
        }
    }
}
