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
        public int MinimalSync() => BuildMinimalConfig(BuildCommand()).Invoke(Args);

        [Benchmark]
        public Task<int> MinimalAsync() => BuildMinimalConfig(BuildCommand()).InvokeAsync(Args);

        private static CliRootCommand BuildCommand()
        {
            CliOption<bool> boolOption = new("--bool", "-b") { Description = "Bool option" };
            CliOption<string> stringOption = new("--string", "-s") { Description = "String option" };

            CliRootCommand command = new()
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

        private static CliConfiguration BuildMinimalConfig(CliCommand command)
        {
            CliConfiguration config = new(command);
            config.Directives.Clear();
            config.EnableDefaultExceptionHandler = false;
            config.ProcessTerminationTimeout = null;
            return config;
        }
    }
}
