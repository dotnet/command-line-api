using System.CommandLine.Tests.Binding;

namespace System.CommandLine.Tests
{
    public class GetValueByNameTypeConversionTests : TypeConversionTests
    {
        protected override T GetValue<T>(CliArgument<T> argument, string commandLine)
        {
            var result = new CliRootCommand { argument }.Parse(commandLine);
            return result.GetValue<T>(argument.Name);
        }

        protected override T GetValue<T>(CliOption<T> option, string commandLine)
        {
            var result = new CliRootCommand { option }.Parse(commandLine);
            return result.GetValue<T>(option.Name);
        }
    }
}
