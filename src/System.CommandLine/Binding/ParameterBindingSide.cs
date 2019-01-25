// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Binding
{

    public class ParameterBindingSide : BindingSide
    {
        private ParameterBindingSide(ParameterInfo parameterInfo, ParameterCollection parameterCollection)
            : base(GetGetter(parameterInfo, parameterCollection), 
                  GetSetter(parameterInfo, parameterCollection))
            => ParameterInfo = parameterInfo;

        public static ParameterBindingSide Create(ParameterInfo parameterInfo, ParameterCollection parameterCollection)
        {
            if (parameterCollection == null)
            {
                throw new ArgumentNullException(nameof(parameterCollection));
            }

            return new ParameterBindingSide(parameterInfo, parameterCollection);
        }

        public ParameterInfo ParameterInfo { get; }

        private static BindingGetter GetGetter(ParameterInfo parameterInfo, ParameterCollection parameterColection)
        {
            parameterColection = parameterColection
                                ?? new ParameterCollection(parameterInfo.Member as MethodBase);
            return (context, target) => parameterColection.GetParameter(parameterInfo);
        }

        private static BindingSetter GetSetter(ParameterInfo parameterInfo, ParameterCollection parameterColection)
        {
            parameterColection = parameterColection
                           ?? new ParameterCollection(parameterInfo.Member as MethodBase);
            return (context, target, value) => parameterColection.SetParameter(parameterInfo, value);
        }
    }
}
