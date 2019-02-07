// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public abstract class MethodBinderBase :ICommandHandler, IOptionBuilder
    {
        private ParameterInfo[] _parameters;

        protected MethodBinderBase(MethodBase method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        protected MethodBase Method { get; }

        protected ParameterInfo[] GetParameters() =>
            _parameters ?? (_parameters = Method.GetParameters());

        public IEnumerable<Option> BuildOptions()
        {
            var optionSet = new SymbolSet();

            foreach (var parameter in GetParameters().OmitInfrastructureTypes())
            {
                optionSet.Add(parameter.BuildOption());
            }

            return optionSet.Cast<Option>();
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            var value = InvokeMethod(context);

            return CommandHandler.GetResultCodeAsync(value, context);
        }

        internal object InvokeMethod(InvocationContext context)
        {
            var parameters = GetParameters().ToArray();

            var arguments = BindMethodArguments(context, parameters);

            return InvokeMethod(arguments);
        }

        protected abstract object InvokeMethod(object[] arguments);

        public static object[] BindMethodArguments(
            InvocationContext context,
            ParameterInfo[] parameters)
        {
            var arguments = new List<object>();

            var commandResult = context.ParseResult.CommandResult;

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameterInfo = parameters[index];

                var typeToResolve = parameterInfo.ParameterType;

                var value = context.ServiceProvider.GetService(typeToResolve);

                if (value == null)
                {
                    var optionResult = Binder.FindMatchingOption(
                        commandResult,
                        parameterInfo.Name);

                    if (optionResult != null)
                    {
                        value = optionResult.GetValueOrDefault();
                    }
                    else if (parameterInfo.Name.IsMatch(
                        commandResult.Command?.Argument?.Name))
                    {
                        value = commandResult.GetValueOrDefault();
                    }
                }

                arguments.Add(value);
            }

            return arguments.ToArray();
        }
    }
}
