using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Builder
{
    public class OptionBuilderTests
    {
        [Fact]
        public void When_option_provide_help_from_WithHelp_the_exposed_help_is_correct()
        {
            var optionBuilder = new OptionBuilder(
                new[] { "-o", "--option" },
                new CommandBuilder("optionCommand"));
            optionBuilder.Description = "desc";
            var option = optionBuilder.WithHelp(new HelpDetail
                                                {
                                                    Name = "helpName",
                                                    Description = "helpDesc",
                                                    IsHidden = true,
                                                }).BuildOption();

            option.Help.Name.Should().Be("helpName");
            option.Help.Description.Should().Be("helpDesc");
            option.Help.IsHidden.Should().BeTrue();
        }

        [Fact]
        public void When_option_provide_help_from_WithHelp_multiple_times_the_exposed_help_is_correct()
        {
            var optionBuilder = new OptionBuilder(
                new[] { "-o", "--option" },
                new CommandBuilder("optionCommand"));

            var option1 = optionBuilder.WithHelp(new HelpDetail
                                                 {
                                                     Name = "helpName1",
                                                     Description = "helpDesc1",
                                                     IsHidden = true,
                                                 }).BuildOption();
            var option2 = optionBuilder.WithHelp(new HelpDetail
                                                 {
                                                     Name = "helpName2",
                                                     Description = "helpDesc2",
                                                     IsHidden = false,
                                                 }).BuildOption();

            option1.Help.Name.Should().Be("helpName1");
            option1.Help.Description.Should().Be("helpDesc1");
            option1.Help.IsHidden.Should().BeTrue();

            option2.Help.Name.Should().Be("helpName2");
            option2.Help.Description.Should().Be("helpDesc2");
            option2.Help.IsHidden.Should().BeFalse();
        }
    }
}
