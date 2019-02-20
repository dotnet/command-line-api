// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ParameterCollection
    {
        private object[] _arguments;
        private ParameterInfo[] _parameters;
        private MethodBase _methodBase;

        public ParameterCollection(MethodBase methodBase)
        {
            _methodBase = methodBase 
                ?? throw new InvalidOperationException("Parameter collection must be initialized with a method")
;
            _parameters = methodBase.GetParameters();
            _arguments = new object[_parameters.Count()];

            for (int i = 0; i < _arguments.Length; i++)
            {
                // Note, the first is the default value for parameter, if set. The second is the default for the type
                _arguments[i] = _parameters[i].HasDefaultValue
                                ? _arguments[i] = _parameters[i].DefaultValue
                                : _parameters[i].ParameterType.GetDefaultValueForType();
            }
        }

        public void SetParameter(ParameterInfo parameterInfo, object value)
        {
            var pos = Array.IndexOf(_parameters, parameterInfo);
            if (pos < 0)
            {
                throw new InvalidOperationException("Unexpected Parameter");
            }
            _arguments[pos] = value;
        }

        public object GetParameter(ParameterInfo parameterInfo)
        {
            var pos = Array.IndexOf(_parameters, parameterInfo);
            if (pos < 0)
            {
                throw new InvalidOperationException("Unexpected Parameter");
            }
            return _arguments[pos];
        }

        public object[] GetArguments()
            => _arguments;
    }
}
