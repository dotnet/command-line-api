using System.Globalization;
using System.Linq;
using Xunit;

namespace System.CommandLine.ApiCompatibility.Tests
{
    public class LocalizationTests
    {
        private const string CommandName = "the-command";

        [Theory]
        [InlineData("es", $"Falta el argumento requerido para el comando: '{CommandName}'.")]
        [InlineData("en-US", $"Required argument missing for command: '{CommandName}'.")]
        public void ErrorMessages_AreLocalized(string cultureName, string expectedMessage)
        {
            CultureInfo uiCultureBefore = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

                CliCommand command = new(CommandName)
                {
                    new CliArgument<string>("arg")
                };

                ParseResult parseResult = command.Parse(CommandName);

                Assert.Equal(expectedMessage, parseResult.Errors.Single().Message);
            }
            finally
            {
                CultureInfo.CurrentUICulture = uiCultureBefore;
            }
        }
    }
}
