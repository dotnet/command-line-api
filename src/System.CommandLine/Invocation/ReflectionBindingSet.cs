using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public class ReflectionBindingSet : BindingSet
    { }
    public class ParameterCollection
    {
        private object[] _arguments;
        private ParameterInfo[] _parameters;
        private MethodBase _methodBase;

        public ParameterCollection(MethodBase methodBase)
        {
            _methodBase = methodBase;
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
            => _arguments[Array.IndexOf(_parameters, parameterInfo)] = value;

        public object GetParameter(ParameterInfo parameterInfo)
            => _arguments[Array.IndexOf(_parameters, parameterInfo)];

        public object[] GetArguments()
            => _arguments;
    }
}
