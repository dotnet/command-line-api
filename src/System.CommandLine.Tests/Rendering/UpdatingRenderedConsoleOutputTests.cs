using System.CommandLine.Rendering;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests.Rendering
{
    public class UpdatingRenderedConsoleOutputTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly ConsoleWriter _consoleWriter;

        public UpdatingRenderedConsoleOutputTests(ITestOutputHelper output)
        {
            _output = output;

            _console = new TestConsole();
            _consoleWriter = new ConsoleWriter(_console);
        }

        [Fact]
        public void Observables_can_be_used_to_trigger_appending_of_additional_output()
        {
            var downloaded = new BehaviorSubject<int>(0);
            var stage = new BehaviorSubject<Stage>(Stage.Starting);

            var model = new ProgressItemViewModel(
                "Progress",
                downloaded,
                1_000_000,
                stage);

            var view = new ProgressItemView(_consoleWriter);
            view.Render(model);

            var outputStep1 = _console.Out.ToString();

            outputStep1
                .Should()
                .Match($"Progress: [ Starting                     ] 0kb / 1000000kb*");

            stage.OnNext(Stage.Downloading);
            downloaded.OnNext(100_000);

            var output = _console.Out.ToString();

            _output.WriteLine(output);

            var outputStep2 = output.Split(Environment.NewLine)[1];

            outputStep2
                .Should()
                .Match($"Progress: [ Downloading ==>                 ] 100000kb / 1000000kb*");
        }
    }

    internal class ProgressItemView : ConsoleView<ProgressItemViewModel>
    {
        public ProgressItemView(ConsoleWriter writer) : base(writer)
        {
        }

        public override void Render(ProgressItemViewModel value)
        {
            value.Stage
                 .Zip(value.DownloadedKb, (stage, downloaded) => (stage, downloaded))
                 .Subscribe(tuple => {
                     var percentage = 20 * tuple.downloaded / value.TotalKb;

                     var progressBar = new string('=', percentage) + (percentage > 0
                                                                          ? ">"
                                                                          : " ");

                     var blankSpace = new string(' ', 20 - percentage - 1);

                     WriteLine(
                         $"{value.Label}: [ {tuple.stage} {progressBar}{blankSpace}] {tuple.downloaded}kb / {value.TotalKb}kb");
                 });
        }
    }

    public class ProgressItemViewModel
    {
        public ProgressItemViewModel(string label, IObservable<int> downloadedKb, int totalKb, IObservable<Stage> stage)
        {
            Label = label;
            DownloadedKb = downloadedKb;
            TotalKb = totalKb;
            Stage = stage;
        }

        public string Label { get; }
        public IObservable<int> DownloadedKb { get; }
        public int TotalKb { get; }
        public IObservable<Stage> Stage { get; }
    }

    public enum Stage
    {
        Starting,
        Downloading,
        Extracting
    }
}
