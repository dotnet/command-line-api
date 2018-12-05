// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public abstract class MethodBinderBase
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

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameterInfo = parameters[index];

                var parameterName = parameterInfo.Name;

                if (parameterInfo.ParameterType == typeof(ParseResult))
                {
                    arguments.Add(context.ParseResult);
                }
                else if (parameterInfo.ParameterType == typeof(InvocationContext))
                {
                    arguments.Add(context);
                }
                else if (parameterInfo.ParameterType == typeof(IConsole))
                {
                    arguments.Add(context.Console);
                }
                else if (parameterInfo.ParameterType == typeof(CancellationToken))
                {
                    CancellationToken ct = context.AddCancellationHandling();
                    arguments.Add(ct);
                }
                else
                {
                    var argument = context.ParseResult
                                          .CommandResult
                                          .ValueForOption(
                                              Binder.FindMatchingOptionName(
                                                  context.ParseResult,
                                                  parameterName));
                    arguments.Add(argument);
                }
            }

            return arguments.ToArray();
        }
    }
}
