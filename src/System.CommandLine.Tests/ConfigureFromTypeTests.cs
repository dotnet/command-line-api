// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ConfigureFromTypeTests
    {
        private readonly TestConsole _testConsole = new TestConsole();
        private readonly ITestOutputHelper _output;

        public ConfigureFromTypeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task ConfigureFromType_GivenSimpleClass_InvokeSuccessfully()
        {
            var builder = new CommandLineBuilder()
                          .ConfigureFromType<WorkingModel>(
                              typeof(WorkingModel).GetMethod(nameof(WorkingModel.Invoke)))
                          .Build();

            int sampleValue = 42;

            var parseResult = builder.Parse($"{CommandLineBuilder.ExeName} --sample-property {sampleValue} --other-property {sampleValue}");

            _output.WriteLine(parseResult.Diagram());

            int result = await builder.InvokeAsync(parseResult, _testConsole);

            result.Should().Be(sampleValue + sampleValue);
        }

        public class WorkingModel
        {
            public WorkingModel(int sampleProperty)
            {
                SampleProperty = sampleProperty;
            }

            public int SampleProperty { get; set; }

            public int OtherProperty { get; set; }

            public int Invoke()
            {
                return SampleProperty + OtherProperty;
            }
        }
    }
}
