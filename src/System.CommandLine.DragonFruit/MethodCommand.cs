using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    internal class MethodCommand
    {
        private readonly MethodInfo _method;
        private readonly OptionDefinition[] _paramOptions;

        public MethodCommand(MethodInfo method, CommandDefinition definition, OptionDefinition[] parameterOptions)
        {
            _method = method ?? throw new ArgumentNullException(nameof(method));
            _paramOptions = parameterOptions ?? throw new ArgumentNullException(nameof(parameterOptions));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public CommandDefinition Definition { get; }

        /// <summary>
        /// Invokes a method command from a given parse result.
        /// </summary>
        /// <param name="object">Can be null for static methods.</param>
        /// <param name="parseResult">The parse result</param>
        /// <returns>The exit code of the application</returns>
        public async Task<int> InvokeMethodAsync(object @object, ParseResult parseResult)
        {
            var rootCommand = parseResult.Command();
            var values = new object[_paramOptions.Length];
            for (var i = 0; i < _paramOptions.Length; i++)
            {
                values[i] = rootCommand.ValueForOption(_paramOptions[i]);
            }

            var retVal = _method.Invoke(@object, values);

            switch (retVal)
            {
                case Task<int> taskOfT:
                    return await taskOfT;
                case Task task:
                    await task;
                    return CommandLine.OkExitCode;
                case int value:
                    return value;
            }

            return CommandLine.OkExitCode;
        }
    }
}
