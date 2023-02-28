using System.CommandLine.Tests.Binding;

namespace System.CommandLine.Tests
{
    public class GetValueByNameTypeConversionTests : TypeConversionTests
    {
        protected override T GetValue<T>(Argument<T> argument, string commandLine)
        {
            var result = new RootCommand { argument }.Parse(commandLine);
            return result.GetValue<T>(argument.Name);
        }

        protected override T GetValue<T>(Option<T> option, string commandLine)
        {
            var result = new RootCommand { option }.Parse(commandLine);
            return result.GetValue<T>(option.Name);
        }
    }
}
