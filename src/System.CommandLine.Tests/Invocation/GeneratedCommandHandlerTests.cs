using FluentAssertions;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public partial class CommandHandlerTests
    {
        [Fact]
        public async Task Can_generate_handler_for_void_returning_method()
        {
            string boundName = default;
            int boundAge = default;
            IConsole boundConsole = null;

            void Execute(string fullnameOrNickname, IConsole console, int age)
            {
                boundName = fullnameOrNickname;
                boundConsole = console;
                boundAge = age;
            }

            var command = new Command("command");
            var nameArgument = new Argument<string>("--name");
            command.AddArgument(nameArgument);
            var ageOption = new Option<int>("--age");
            command.AddOption(ageOption);

            command.Handler = CommandHandler.Generator.Generate<Action<string, IConsole, int>>
                (Execute, nameArgument, ageOption);

            await command.InvokeAsync("command Gandalf --age 425", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_method_with_model()
        {
            string boundName = default;
            int boundAge = default;
            IConsole boundConsole = null;

            void Execute(Character character, IConsole console)
            {
                boundName = character.FullName;
                boundConsole = console;
                boundAge = character.Age;
            }

            var command = new Command("command");
            var nameOption = new Option<string>("--name");
            command.AddOption(nameOption);
            var ageOption = new Option<int>("--age");
            command.AddOption(ageOption);

            command.Handler = CommandHandler.Generator.Generate<Action<Character, IConsole>>
                (Execute, nameOption, ageOption);

            await command.InvokeAsync("command --age 425 --name Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_method_with_model_property_binding()
        {
            string boundName = default;
            int boundAge = default;
            IConsole boundConsole = null;

            void Execute(Character character, IConsole console)
            {
                boundName = character.FullName;
                boundConsole = console;
                boundAge = character.Age;
            }

            var command = new Command("command");
            var nameOption = new Option<string>("--name");
            command.AddOption(nameOption);
            var ageOption = new Option<int>("--age");
            command.AddOption(ageOption);

            command.Handler = CommandHandler.Generator.Generate<Action<Character, IConsole>, Character>
                (Execute, context => new Character()
                {
                    FullName = context.ParseResult.ValueForOption(nameOption),
                    Age = context.ParseResult.ValueForOption(ageOption),
                });

            await command.InvokeAsync("command --age 425 --name Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        public class Character
        {
            public Character(string fullName, int age)
            {
                FullName = fullName;
                Age = age;
            }

            public Character()
            { }

            public string FullName { get; set; }
            public int Age { get; set; }
        }

    }
}
