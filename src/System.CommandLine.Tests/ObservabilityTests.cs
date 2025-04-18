using System.CommandLine.Parsing;
using FluentAssertions;
using System.Linq;
using Xunit;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ObservabilityTests
    {
        private readonly ITestOutputHelper log;

        public ObservabilityTests(ITestOutputHelper output)
        {
            log = output;
        }

        [Fact]
        public void It_creates_activity_spans_for_parsing()
        {
            var (listener, activities) = SetupListener();

            var command = new Command("the-command")
            {
                new Option<string>("--option")
            };

            var args = new[] { "--option", "the-argument" };

            var result = command.Parse(args);
            listener.Dispose();
            activities
                    .Should()
                    .ContainSingle(
                        a => a.OperationName == "System.CommandLine.Parse"
                             && a.Status == ActivityStatusCode.Ok
                             && a.Tags.Any(t => t.Key == "command" && t.Value == "the-command"));
        }

        [Fact]
        public void It_creates_activity_spans_for_parsing_errors()
        {
            var (listener, activities) = SetupListener();

            var command = new Command("the-command")
            {
                new Option<string>("--option")
            };

            var args = new[] { "--opt", "the-argument" };
            var result = command.Parse(args);
            listener.Dispose();
            activities
                    .Should()
                    .ContainSingle(
                        a => a.OperationName == "System.CommandLine.Parse"
                             && a.Status == ActivityStatusCode.Error
                             && a.Tags.Any(t => t.Key == "command" && t.Value == "the-command")
                             && a.Baggage.Any(t => t.Key == "errors"));
        }

        [Fact]
        public async Task It_creates_activity_spans_for_invocations()
        {
            var (listener, activities) = SetupListener();

            var command = new Command("the-command");
            command.SetAction(async (pr, ctok) => await Task.FromResult(0));

            var result = await command.Parse(Array.Empty<string>()).InvokeAsync();
            listener.Dispose();

            activities
                    .Should()
                    .ContainSingle(
                        a => a.OperationName == "System.CommandLine.Invoke"
                             && a.DisplayName == "the-command"
                             && a.Status == ActivityStatusCode.Ok
                             && a.Tags.Any(t => t.Key == "command" && t.Value == "the-command")
                             && a.Tags.Any(t => t.Key == "invoke.type" && t.Value == "async")
                             && a.TagObjects.Any(t => t.Key == "exitcode" && (int)t.Value == 0));
        }

        [Fact]
        public async Task It_creates_activity_spans_for_invocation_errors()
        {
            var (listener, activities) = SetupListener();

            var command = new Command("the-command");
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            command.SetAction(async (pr, ctok) =>
            {
                throw new Exception("Something went wrong");
            });
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            var result = await command.Parse(Array.Empty<string>()).InvokeAsync();
            listener.Dispose();

            foreach (var x in activities)
            {
                log.WriteLine($"{x.DisplayName}({x.OperationName})/{x.Status}({x.Duration}) - {x.TagObjects} - {string.Join(",", x.Events.Select((k) => $"{k.Name},{k.Tags}"))}");
            }

            activities
                    .Should()
                    .ContainSingle(
                        a => a.OperationName == "System.CommandLine.Invoke"
                             && a.DisplayName == "the-command"
                             && a.Status == ActivityStatusCode.Error
                             && a.Tags.Any(t => t.Key == "command" && t.Value == "the-command")
                             && a.Tags.Any(t => t.Key == "invoke.type" && t.Value == "async")
                             && a.TagObjects.Any(t => t.Key == "exitcode" && (int)t.Value == 1)
                             && a.Events.Any(t => t.Name == "exception"));
        }

        private static (ActivityListener, List<Activity>) SetupListener()
        {
            List<Activity> activities = new();
            var listener = new ActivityListener();
            listener.ShouldListenTo = s => true;
            listener.Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData;
            listener.ActivityStopped = a => activities.Add(a);
            ActivitySource.AddActivityListener(listener);
            return new(listener, activities);
        }
    }
}
