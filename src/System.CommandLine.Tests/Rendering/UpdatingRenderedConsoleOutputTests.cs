using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
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
        private readonly ConsoleRenderer _consoleRenderer;

        public UpdatingRenderedConsoleOutputTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole();
            _console.Height = 1;
            _consoleRenderer = new ConsoleRenderer(_console);
        }

        [Fact(Skip = "WIP")]
        public void Observables_can_be_used_to_trigger_appending_of_additional_output()
        {
            var downloaded = new BehaviorSubject<int>(0);
            var stage = new BehaviorSubject<Stage>(Stage.Starting);

            var model = new ProgressItemViewModel(
                "Progress",
                downloaded,
                1_000_000,
                stage);

            var view = new ProgressItemView(model);
            view.Render(_consoleRenderer, new Region(0, 0, 53, 1));

            var outputStep1 = _console.Out.ToString();

            outputStep1
                .Should()
                .Match($"Progress: [ Starting*] 0kb / 1000000kb*");

            stage.OnNext(Stage.Downloading);
            downloaded.OnNext(100_000);

            var output = _console.Out.ToString();

            _output.WriteLine(output);

            var outputStep2 = output.Split(Environment.NewLine)[1];

            outputStep2
                .Should()
                .Match($"Progress: [ Downloading ==>*] 100000kb / 1000000kb*");
        }
    }

    internal class ProgressItemView : ContentView
    {
        public ProgressItemView(ProgressItemViewModel value)
        {
            var observable = value.Stage
                 .Zip(value.DownloadedKb, (stage, downloaded) => (stage, downloaded));

            Observe(observable, tuple =>
            {
                var percentage = 20 * tuple.downloaded / value.TotalKb;

                var progressBar = new string('=', percentage) + (percentage > 0
                                                                     ? ">"
                                                                     : " ");
                var blankSpace = new string(' ', 20 - percentage - 1);

                return
                    $"{value.Label}: [ {tuple.stage} {progressBar}{blankSpace}] {tuple.downloaded}kb / {value.TotalKb}kb";
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
